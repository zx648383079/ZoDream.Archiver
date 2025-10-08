using System;
using System.Linq;
using System.Threading;
using ZoDream.BundleExtractor.Producers;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Logging;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor
{
    public partial class BundleReader(IBundleSource fileItems, 
        IBundleOptions options, BundleScheme scheme) : IBundleHandler
    {

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            // 从配置获取引擎
            var engine = scheme.Get<IBundleEngine>(options);
            if (engine is null)
            {
                return;
            }
            TryLoad();
            //Parallel.ForEach(
            //    engine.EnumerateChunk(fileItems, options),
            //    new ParallelOptions()
            //    {
            //        MaxDegreeOfParallelism = 3
            //    }, items => {

            //    });
            var onlyDependencyTask = options is IBundleExtractOptions o && o.OnlyDependencyTask;
            var logger = scheme.Service.Get<ILogger>();
            logger.Info($"Engine: {engine.AliasName}");
            logger.Info("Analyzing ...");
            fileItems = engine.Unpack(fileItems, options);
            fileItems.Analyze(token);
            logger.Info($"Found {fileItems.Count} files.");
            var service = scheme.Service;
            var progress = logger.CreateProgress("Extract Chunk ...", fileItems.Count);
            if (!onlyDependencyTask && service.TryLoadPoint(fileItems.GetHashCode(), out var record))
            {
                progress.Value = (int)record;
            }
            if (fileItems.Count == 0)
            {
                return;
            }
            var builder = engine.GetBuilder(options);
            scheme.Service.Add(builder);
            var temporary = scheme.Service.Get<ITemporaryStorage>();
            var splitter = engine.CreateSplitter(options);
            foreach (var item in fileItems.GetFiles().Skip((int)progress.Value).Select(i => new FilePath(i)))
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                if (!splitter.TrySplit(item, fileItems, out var next))
                {
                    continue;
                }
                logger.Info($"Extract {next.Count} files ...");
                try
                {
                    using var hander = engine.CreateHandler(next, options);
                    hander?.ExtractTo(folder, mode, token);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }
                temporary.Clear();
                builder?.Flush();
                progress.Add(next.Count);
                if (!onlyDependencyTask)
                {
                    service?.SavePoint(fileItems.GetHashCode(), (uint)progress.Value);
                }
            }
            builder?.Dispose();
        }
        /// <summary>
        /// 局部配置初始化
        /// </summary>
        private void TryLoad()
        {
            // 从配置获取制作者
            var producer = scheme.Get<IBundleProducer>(options);
            var service = scheme.Service;
            service.Add(options);
            service.Add(UnknownProducer.CheckKey, producer is UnknownProducer || producer is null);
            if (producer is null)
            {
                service.Add<IBundleSerializer>(service.Get<BundleSerializer>());
                service.Add<IBundleParser>(service.Get<BundleStorage>());
                return;
            }
            AddProducer(service, producer);
        }

        public void Dispose()
        {
        }

        internal static void AddProducer(IEntryService service, 
            IBundleProducer producer)
        {
            var options = service.Get<IBundleOptions>();
            var instance = producer.CreateSerializer(options);
            service.Add(instance);
            var last = instance.Converters.Last();
            var storage = last is IBundleParser s ? s : producer.CreateParser(options);
            service.Add(storage);
            service.Add(storage is IBundleCodec codec ? codec : service.Get<BundleCodec>());
            service.Get<ILogger>().Info($"Producer: {producer.AliasName}");
        }
    }
}
