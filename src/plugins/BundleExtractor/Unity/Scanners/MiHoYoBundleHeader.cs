using System.Diagnostics;
using ZoDream.BundleExtractor.Unity.BundleFiles;
using ZoDream.Shared;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public class MiHoYoBundleHeader : BundleHeader
    {
        internal const string UnityArchiveMagic = "mhy0";
        protected override string MagicString => UnityArchiveMagic;
        public long Size { get; set; }
        public int CompressedBlocksInfoSize { get; set; }
        public int UncompressedBlocksInfoSize { get; set; }

        public BundleFlags Flags { get; set; }

        public override void Read(IBundleBinaryReader reader)
        {
            var signature = reader.ReadStringZeroTerm();
            Expectation.ThrowIfNotSignature(signature == MagicString);
            Version = UnityBundleVersion.BF_520_x;
            UnityWebBundleVersion = "5.x.x";
            UnityWebMinimumRevision = "2017.4.30f1";
            Flags = (BundleFlags)0x43;
            var size = reader.ReadUInt32();
            UncompressedBlocksInfoSize = CompressedBlocksInfoSize = (int)size;
        }
    }
}
