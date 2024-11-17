using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Platforms
{
    public class WiiUPlatformScheme : IBundlePlatform
    {
        internal const string PlatformName = "WiiU"; 
        public string AliasName => PlatformName;

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
