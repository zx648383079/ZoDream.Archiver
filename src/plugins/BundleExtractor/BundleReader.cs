using System;
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
                    scheme.Service.Get<ILogger>().Error(ex.Message);
                }
            }
        }
        /// <summary>
        /// 局部配置初始化
        /// </summary>
        private void TryLoad()
        {
            // 从配置获取制作者
            var producer = scheme.Get<IBundleProducer>(options);
            if (producer is null)
            {
                scheme.Service.Add<IBundleElementScanner>(new BundleElementScanner());
                scheme.Service.Add<IBundleStorage>(new BundleStorage());
                return;
            }
            var instance = producer.GetScanner(options);
            scheme.Service.Add<IBundleElementScanner>(instance);
            var storage = instance is IBundleStorage s ? s : producer.GetStorage(options);
            scheme.Service.Add<IBundleStorage>(storage);
            scheme.Service.Add<IBundleCodec>(storage is IBundleCodec codec ? codec : new BundleCodec());
        }

        public void Dispose()
        {
        }
    }
}
