using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Platforms
{
    public class UnknownPlatform : IBundlePlatform
    {
        public string AliasName => string.Empty;
        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
