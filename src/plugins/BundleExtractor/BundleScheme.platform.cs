using System.Linq;
using ZoDream.BundleExtractor.Platforms;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme
    {
        private readonly IBundlePlatform[] _platformItems = [
            new WindowsPlatformScheme(),
            new AndroidPlatformScheme(),
            new IosPlatformScheme(),
            new LinuxPlatformScheme(),
            new MacPlatformScheme(),
            new PlayStationPlatformScheme(),
            new SwitchPlatformScheme(),
            new WebGLPlatformScheme(),
            new WiiUPlatformScheme(),
            new UnknownPlatform(),
        ];

        public string[] PlatformNames => GetNames(_platformItems);
    }
}
