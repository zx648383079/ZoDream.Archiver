using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Platforms
{
    public class IosPlatformScheme : IBundlePlatform
    {
        public const string PlatformName = "IOS";

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
