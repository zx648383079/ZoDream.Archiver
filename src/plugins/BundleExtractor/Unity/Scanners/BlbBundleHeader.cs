using System.Diagnostics;
using ZoDream.BundleExtractor.Unity.BundleFiles;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public class BlbBundleHeader : BundleHeader
    {
        internal const string UnityArchiveMagic = "Blb\x02";
        protected override string MagicString => UnityArchiveMagic;
        public long Size { get; set; }
        public int CompressedBlocksInfoSize { get; set; }
        public int UncompressedBlocksInfoSize { get; set; }

        public override void Read(IBundleBinaryReader reader)
        {
            var signature = reader.ReadStringZeroTerm();
            Debug.Assert(signature == MagicString);
            Version = UnityBundleVersion.BF_520_x;
            UnityWebBundleVersion = "5.x.x";
            UnityWebMinimumRevision = "2017.4.30f1";
            var size = reader.ReadUInt32();
            UncompressedBlocksInfoSize = CompressedBlocksInfoSize = (int)size;
        }
    }
}
