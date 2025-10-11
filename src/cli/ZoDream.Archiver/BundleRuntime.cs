using ZoDream.BundleExtractor;
using ZoDream.BundleExtractor.Unity;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Logging;
using ZoDream.Shared.Models;

namespace ZoDream.Archiver
{
    public class BundleRuntime(
        string rootFolder, 
        BundleOptions options,
        int skipCount = 0
        ) : IConsoleRuntime
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
            var logger = new ConsoleLogger();
            if (!Directory.Exists(options.Entrance))
            {
                logger.Error($"<{options.Entrance}> Not Found!");
                return;
            }
            var entryFolder = new List<string>
            {
                options.Entrance
            };
            var extraFolder = Path.Combine(rootFolder, "files");
            if (Directory.Exists(extraFolder))
            {
                entryFolder.Add(extraFolder);
            }
            IBundleSource source = new BundleSource(entryFolder);
            using var service = new BundleService();
            using var temporary = new TemporaryStorage();
            service.Add<ITemporaryStorage>(temporary);
            service.Add<ILogger>(logger);

            using var scheme = new BundleScheme(service);
            var extraPackage = options.Package;
            scheme.LoadEnvironment(source, options);
            if (!string.IsNullOrWhiteSpace(extraPackage) && extraPackage != options.Package)
            {
                options.Package += "." + extraPackage;
            }
            var engine = scheme.Get<IBundleEngine>(options);
            var producer = scheme.Get<IBundleProducer>(options);

            var scanner =  producer.CreateParser(options);//new QooElementScanner(source, options);
    
            service.Add<IBundleOptions>(options);
            service.Add<IBundleProducer>(producer);
            service.Add<IBundleEngine>(engine);
            service.Add<IBundlePlatform>(scheme.Get<IBundlePlatform>(options));
            service.Add<IBundleParser>(scanner);

            service.Add<IBundleCodec>(new BundleCodec());

            IBundleSerializer serializer;
            if (!string.IsNullOrWhiteSpace(options.TypeTree) && engine is BundleExtractor.Engines.UnityEngine)
            {
                serializer = TypeTreeSerializer.CreateForm(options.TypeTree);
            }
            else
            {
                serializer = producer.CreateSerializer(options);
            }
            service.Add(serializer);

            var builder = engine.GetBuilder(options);
            service.Add(builder);
            Console.WriteLine($"Entrance: {options.Entrance}");
            if (!string.IsNullOrWhiteSpace(options.Package))
            {
                Console.WriteLine($"          {options.Package} {options.DisplayName}");
            }
            Console.WriteLine($"          {options.Platform} {options.Engine}");
            if (!string.IsNullOrWhiteSpace(options.DependencySource))
            {
                Console.WriteLine($"Dependency: {options.DependencySource}");
            }
            Console.WriteLine($"Output: {options.OutputFolder}");
            


            logger.Info("Analyzing ...");
            source = engine.Unpack(source, options);
            source.Analyze(token);
            logger.Info($"Found {source.Count} files.");
            var splitter = engine.CreateSplitter(options);
            var filter = new BundleMultipleFilter();
            if (engine is IBundleFilter ef)
            {
                filter.Add(ef);
            }
            if (scanner is IBundleFilter f)
            {
                filter.Add(f);
            }
            var progress = logger.CreateProgress(options.OnlyDependencyTask ? "Build Dependency" : "Extract Chunk", source.Count);
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
    }
}
