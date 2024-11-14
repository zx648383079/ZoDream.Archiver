using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.BundleExtractor.Producers;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Platforms
{
    public class WindowsPlatformScheme : IBundlePlatform
    {
        public const string PlatformName = "Windows";

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
