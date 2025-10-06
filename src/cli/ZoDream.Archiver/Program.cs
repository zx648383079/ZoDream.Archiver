using ZoDream.BundleExtractor;
using ZoDream.BundleExtractor.Platforms;
using ZoDream.BundleExtractor.Producers;
using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Logging;
using ZoDream.Shared.Models;

namespace ZoDream.Archiver
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var logger = new EventLogger();
            logger.OnLog += Logger_OnLog;
            logger.OnProgress += Logger_OnProgress;
            var options = new BundleOptions()
            {
                OutputFolder = "output"
            };
            IBundleSource source = new BundleSource(["hhh"]);
            var service = new BundleService();
            var engine = new BundleExtractor.Engines.UnityEngine(service);
            var scanner = new QooElementScanner(source, options);
            service.Add<ITemporaryStorage>(new TemporaryStorage());
            service.Add<ILogger>(logger);
            service.Add<IBundleOptions>(options);
            service.Add<IBundleProducer>(new UnknownProducer());
            service.Add<IBundlePlatform>(new AndroidPlatformScheme());
            service.Add<IBundleEngine>(engine);
            service.Add<IBundleParser>(scanner);
            service.Add<IBundleCodec>(new BundleCodec());

            logger.Info("Analyzing ...");
            source = engine.Unpack(source, options);
            source.Analyze();
            logger.Info($"Found {source.Count} files.");
            var chunk = engine.CreateSplitter(options);
            var index = 0;
            var filter = new BundleMultipleFilter([engine, scanner]);
            var progress = logger.CreateProgress("Extract Chunk ...", source.Count);
            foreach (var item in source.GetFiles().Skip((int)progress.Value).Select(i => new FilePath(i)))
            {
                if (filter.IsMatch(item))
                {
                    index++;
                    continue;
                }
                if (!chunk.TrySplit(item, source, out var next))
                {
                    continue;
                }
                logger.Info($"Extract {next.Count} files ...");
                var hander = engine.CreateHandler(next, options);
                hander.ExtractTo(options.OutputFolder, ArchiveExtractMode.Overwrite);
                progress.Value += next.Count;
            }
            Console.ReadKey();
        }

        private static void Logger_OnProgress(ProgressLogger progress)
        {
            lock (Console.Out)
            {
                int originalLeft = Console.CursorLeft;
                int originalTop = Console.CursorTop;

                Console.SetCursorPosition(0, progress.IsMaster ? 1 : 2);

                var val = (int)((double)progress / 5);
                Console.Write($"{progress.Title}: [");
                Console.Write(new string('=', val));
                Console.Write(new string(' ', 20 - val));
                Console.Write($"] {progress.Value}/{progress.Max}");

                // 清除行尾
                Console.Write(new string(' ', 10));

                Console.SetCursorPosition(originalLeft, originalTop);
            }
        }

        private static void Logger_OnLog(string message, LogLevel level)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]{level}:{message}");
        }
    }
}
