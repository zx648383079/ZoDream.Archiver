using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.BundleExtractor.Unity.BundleFiles;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Producers
{
    public class MhyBundleHeader : BundleHeader
    {
        internal const string UnityArchiveMagic = "mhy0";
        protected override string MagicString => UnityArchiveMagic;
        public long Size { get; set; }
        public int CompressedBlocksInfoSize { get; set; }
        public int UncompressedBlocksInfoSize { get; set; }

        public BundleFlags Flags { get; set; }

        public override void Read(EndianReader reader)
        {
            var signature = reader.ReadStringZeroTerm();
            Debug.Assert(signature == MagicString);
            Version = BundleVersion.BF_520_x;
            UnityWebBundleVersion = "5.x.x";
            UnityWebMinimumRevision = "2017.4.30f1";
            Flags = (BundleFlags)0x43;
            var size = reader.ReadUInt32();
            UncompressedBlocksInfoSize = CompressedBlocksInfoSize = (int)size;
        }
    }
}
