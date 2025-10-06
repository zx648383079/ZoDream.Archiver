using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.BundleExtractor.Unity.YooAsset;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Logging;
using ZoDream.Shared.Models;

namespace ZoDream.Tests
{
    [TestClass]
    public class BundleTest
    {
        [TestMethod]
        public void TestFlow()
        {
            var options = new BundleOptions();
            var service = new BundleService();
            service.Add<ITemporaryStorage>(new TemporaryStorage());
            service.Add<ILogger>(new EventLogger());
            service.Add<IBundleOptions>(options);
            var folder = "output";
            IBundleSource source = new BundleSource(["hhh"]);
            if (true)
            {
                source = new YooAssetScheme(source);
            }
            IBundleEngine engine = new BundleExtractor.Engines.UnityEngine(service);
            var chunk = engine.CreateSplitter(options);
            var index = 0;
            foreach (var item in chunk.Split(source))
            {
                index += item.Count;
                var reader = engine.CreateHandler(item, options);
                reader.ExtractTo(folder, ArchiveExtractMode.Overwrite);
            }
            IBundleFilter filter = (IBundleFilter)engine;
            index = 5;
            foreach (var item in source.GetFiles().Skip(index).Select(i => new FilePath(i)))
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
                index+= next.Count;
                var hander = engine.CreateHandler(next, options);
                hander.ExtractTo(folder, ArchiveExtractMode.Overwrite);
            }
        }
    }
}
