using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Platforms
{
    public class SwitchPlatformScheme : IBundlePlatform
    {
        internal const string PlatformName = "Switch"; 
        public string AliasName => PlatformName;

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
