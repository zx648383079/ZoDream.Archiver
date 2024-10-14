using ZoDream.Shared.Interfaces;
using ZoDream.BundleExtractor.Platforms;
using System.Collections.Generic;
using ZoDream.BundleExtractor.Producers;
using ZoDream.BundleExtractor.Engines;

namespace ZoDream.BundleExtractor
{
    public class BundleScheme : IBundleScheme
    {
        #region BundleReader
        public IBundleReader? Load(IEnumerable<string> fileItems, IArchiveOptions? options = null)
        {
            var platform = GetPlatform(fileItems);
            if (platform == null)
            {
                return null;
            }
            return new BundleReader(this, platform, options);
        }

        internal static IBundlePlatform? GetPlatform(IEnumerable<string> fileItems)
        {
            IBundlePlatform[] platforms = [
                new WindowsPlatformScheme(),
                new AndroidPlatformScheme(),
                new IosPlatformScheme(),
            ];
            foreach (var item in platforms)
            {
                if (item.TryLoad(fileItems))
                {
                    return item;
                }
            }
            return null;
        }

        internal static IBundleProducer GetProducer(IEnumerable<string> fileItems)
        {
            IBundleProducer[] producers = [
                new MiHoYoProducer(),
                new PaperProducer()
            ];
            foreach (var item in producers)
            {
                if (item.TryLoad(fileItems))
                {
                    return item;
                }
            }
            return new UnityProducer();
        }

        internal static IBundleEngine GetEngine(
            IBundlePlatform platform,
            IEnumerable<string> fileItems)
        {
            IBundleEngine[] items = [
                new UnityEngine(platform),
                new CocosEngine(platform)
            ];
            foreach (var item in items)
            {
                if (item.TryLoad(fileItems))
                {
                    return item;
                }
            }
            return new UnknownEngine();
        }

        #endregion


    }
}
