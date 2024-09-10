using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.BundleFiles
{
    public class ArchiveBundleHeader : BundleHeader
    {
        private const string UnityArchiveMagic = "UnityArchive";
        protected override string MagicString => UnityArchiveMagic;
    }
}
