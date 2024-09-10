using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.BundleFiles
{
    public class WebBundleHeader : RawWebBundleHeader
    {
        private const string UnityWebMagic = "UnityWeb";
        protected override string MagicString => UnityWebMagic;
    }
}
