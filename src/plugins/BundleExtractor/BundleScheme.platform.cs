using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme
    {
        private IBundlePlatform[] _platformItems = [];

        public string[] PlatformNames => GetNames(_platformItems);
    }
}
