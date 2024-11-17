using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Platforms
{
    public class WebGLPlatformScheme : IBundlePlatform
    {
        internal const string PlatformName = "WebGL"; 
        public string AliasName => PlatformName;

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
