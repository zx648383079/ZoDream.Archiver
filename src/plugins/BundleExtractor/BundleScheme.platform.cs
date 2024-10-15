using ZoDream.Shared.Interfaces;
using ZoDream.BundleExtractor.Platforms;
using System.Collections.Generic;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme
    {
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

    }
}
