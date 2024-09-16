using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.BundleFiles
{
    public class RawBundleHeader: RawWebBundleHeader
    {
        internal const string UnityRawMagic = "UnityRaw";
        protected override string MagicString => UnityRawMagic;
    }
}
