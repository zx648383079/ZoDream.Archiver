using System;
using ZoDream.BundleExtractor.Platforms;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme
    {

        internal static IBundlePlatform CreatePlatform(IBundleOptions options)
        {
            return options.Platform switch
            {
                WindowsPlatformScheme.PlatformName => new WindowsPlatformScheme(),
                AndroidPlatformScheme.PlatformName => new AndroidPlatformScheme(),
                IosPlatformScheme.PlatformName => new IosPlatformScheme(),
                _ => throw new NotImplementedException(),
            };
        }

        internal static bool TryGetPlatform(
            IBundleSource fileItems, IBundleOptions options)
        {
            IBundlePlatform[] platforms = [
                new WindowsPlatformScheme(),
                new AndroidPlatformScheme(),
                new IosPlatformScheme(),
            ];
            foreach (var item in platforms)
            {
                if (item.TryLoad(fileItems, options))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
