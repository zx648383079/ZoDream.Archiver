using System.IO;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Platforms
{
    public class WindowsPlatformScheme : IBundlePlatform
    {
        internal const string PlatformName = "Windows";

        public string AliasName => PlatformName;

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            foreach (var item in fileItems.GetFiles("*.exe"))
            {
                options.Platform = PlatformName;
                options.Entrance = Path.GetDirectoryName(item);
                return true;
            }
            return false;
        }
    }
}
