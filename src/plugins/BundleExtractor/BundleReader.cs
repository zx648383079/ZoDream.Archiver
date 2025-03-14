﻿using System;
using System.Threading;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor
{
    public class BundleReader(BundleSource fileItems, 
        IBundleOptions options, BundleScheme scheme) : IBundleReader
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
            var logger = scheme.Service.Get<ILogger>();
            
            logger.Info("Analyzing ...");
            fileItems.Analyze(token);
            logger.Info($"Found {fileItems.Count} files.");
            var progress = 0;
            logger.Progress(progress, fileItems.Count);
            if (fileItems.Count == 0)
            {
                return;
            }
            var builder = engine.GetBuilder(options);
            scheme.Service.Add(builder);
            var temporary = scheme.Service.Get<ITemporaryStorage>();
            foreach (var items in engine.EnumerateChunk(fileItems, options))
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                try
                {
                    using var chunk = engine.OpenRead(items, options);
                    chunk?.ExtractTo(folder, mode, token);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }
                temporary.Clear();
                builder?.Flush();
                progress += items.Index;
                logger.Progress(progress, fileItems.Count);
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
            if (producer is null)
            {
                service.Add<IBundleElementScanner>(service.Get<BundleElementScanner>());
                service.Add<IBundleStorage>(service.Get<BundleStorage>());
                return;
            }
            var instance = producer.GetScanner(options);
            service.Add<IBundleElementScanner>(instance);
            var storage = instance is IBundleStorage s ? s : producer.GetStorage(options);
            service.Add<IBundleStorage>(storage);
            service.Add<IBundleCodec>(storage is IBundleCodec codec ? codec : service.Get<BundleCodec>());
        }

        public void Dispose()
        {
        }
    }
}
