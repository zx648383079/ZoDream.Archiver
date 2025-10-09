using ZoDream.BundleExtractor;
using ZoDream.BundleExtractor.Platforms;
using ZoDream.BundleExtractor.Producers;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Logging;
using ZoDream.Shared.Models;

namespace ZoDream.Archiver
{
    public class BundleRuntime(string rootFolder, int skipCount = 0) : IConsoleRuntime
    {
        public Task RunAsync(CancellationToken token = default)
        {
            return Task.Factory.StartNew(() => {
                Run(token);
            }, token);
        }

        public void Run(CancellationToken token = default)
        {
            Console.WriteLine();
            Console.WriteLine();
            var logger = new EventLogger();
            logger.OnLog += Logger_OnLog;
            logger.OnProgress += Logger_OnProgress;
            var options = new BundleOptions()
            {
                Platform = "Android",
                Engine = "Unity",
                //Package = "fake",
                FileMode = ArchiveExtractMode.Overwrite,
                OutputFolder = Path.Combine(rootFolder, "output"),
                Entrance = Path.Combine(rootFolder, "resources"),
                ModelFormat = "gltf",
                DependencySource = Path.Combine(rootFolder, "dependencies.bin"),
                OnlyDependencyTask = false,
                MaxBatchCount = 10,
            };
            if (!Directory.Exists(options.Entrance))
            {
                logger.Error($"<{options.Entrance}> Not Found!");
                return;
            }
            IBundleSource source = new BundleSource([
                options.Entrance,
                Path.Combine(rootFolder, "files")
            ]);
            var service = new BundleService();
            var engine = new BundleExtractor.Engines.UnityEngine(service);
            var producer = new UnknownProducer();

            var scanner =  producer.CreateParser(options);//new QooElementScanner(source, options);
            var temporary = new TemporaryStorage();
            service.Add<ITemporaryStorage>(temporary);
            service.Add<ILogger>(logger);
            service.Add<IBundleOptions>(options);
            service.Add<IBundleProducer>(producer);
            service.Add<IBundlePlatform>(new AndroidPlatformScheme());
            service.Add<IBundleEngine>(engine);
            service.Add<IBundleParser>(scanner);

            service.Add<IBundleCodec>(new BundleCodec());
            service.Add(producer.CreateSerializer(options));

            var builder = engine.GetBuilder(options);
            service.Add(builder);

            logger.Info("Analyzing ...");
            source = engine.Unpack(source, options);
            source.Analyze(token);
            logger.Info($"Found {source.Count} files.");
            var splitter = engine.CreateSplitter(options);
            var filter = new BundleMultipleFilter([engine]);
            if (scanner is IBundleFilter f)
            {
                filter.Add(f);
            }
            var progress = logger.CreateProgress("Extract Chunk ...", source.Count);
            progress.Value = skipCount;
            if (progress.Value > 0)
            {
                logger.Info($"Skip {progress.Value} files.");
            }
            IBundleChunk? next;
            foreach (var item in source.GetFiles().Skip((int)progress.Value).Select(i => new FilePath(i)))
            {
                if (filter.IsMatch(item))
                {
                    progress.Add(1);
                    continue;
                }
                if (!splitter.TrySplit(item, source, out next))
                {
                    continue;
                }
                logger.Info($"Extract {next.Count} files ...");
                var hander = engine.CreateHandler(next, options);
                hander.ExtractTo(options.OutputFolder, ArchiveExtractMode.Overwrite, token);
                temporary.Clear();
                builder?.Flush();
                progress.Add(next.Count);
            }
            if (splitter.TryFinish(source, out next))
            {
                logger.Info($"Extract {next.Count} files ...");
                var hander = engine.CreateHandler(next, options);
                hander.ExtractTo(options.OutputFolder, ArchiveExtractMode.Overwrite, token);
                temporary.Clear();
                builder?.Flush();
                progress.Add(next.Count);
            }
            builder?.Dispose();
            logger.Info($"Finished!");
        }

        private void Logger_OnProgress(ProgressLogger progress)
        {
            lock (Console.Out)
            {
                int originalLeft = Console.CursorLeft;
                int originalTop = Console.CursorTop;

                Console.SetCursorPosition(0, progress.IsMaster ? 0 : 1);

                var val = (int)((double)progress / 5);
                var text = $"{progress.Title}: [{new string('=', val)}{new string(' ', 20 - val)}] {progress.Value}/{progress.Max}";
                Console.Write(text);
                if (text.Length < Console.BufferWidth)
                {
                    Console.Write(new string(' ', Console.BufferWidth - text.Length));
                }
                Console.SetCursorPosition(originalLeft, originalTop);
            }
        }

        private void Logger_OnLog(string message, LogLevel level)
        {
            var originalColor = Console.ForegroundColor;
            Console.Write($"[{DateTime.Now:HH:mm:ss}]{level}:");
            Console.ForegroundColor = level switch
            {
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Warn => ConsoleColor.Yellow,
                _ => originalColor
            };
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }
    }
}
