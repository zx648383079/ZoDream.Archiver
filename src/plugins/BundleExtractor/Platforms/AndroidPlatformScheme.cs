using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Platforms
{
    public class AndroidPlatformScheme: IPlatformScheme
    {
        private const string AssetName = "assets";
        private const string MetaName = "META-INF";
        private const string BinName = "bin";
        private const string Il2CppGameAssemblyName = "libil2cpp.so";
        private const string AndroidUnityAssemblyName = "libunity.so";


    }
}
