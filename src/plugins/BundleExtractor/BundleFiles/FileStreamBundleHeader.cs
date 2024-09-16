using System.Diagnostics;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.BundleFiles
{
    public class FileStreamBundleHeader: BundleHeader
    {
        internal const string UnityFSMagic = "UnityFS";
        protected override string MagicString => UnityFSMagic;
        /// <summary>
        /// Equal to file size, sometimes equal to uncompressed data size without the header
        /// </summary>
        public long Size { get; set; }
        /// <summary>
        /// UnityFS length of the possibly-compressed (LZMA, LZ4) bundle data header
        /// </summary>
        public int CompressedBlocksInfoSize { get; set; }
        public int UncompressedBlocksInfoSize { get; set; }
        public BundleFlags Flags { get; set; }

        public CompressionType CompressionType {
            get {
                return (CompressionType)(Flags & BundleFlags.CompressionTypeMask);
            }
            set {
                Flags = (Flags & ~BundleFlags.CompressionTypeMask) | (BundleFlags.CompressionTypeMask & (BundleFlags)value);
            }
        }

        public override void Read(EndianReader reader)
        {
            base.Read(reader);
            Size = reader.ReadInt64();
            Debug.Assert(Size >= 0);
            CompressedBlocksInfoSize = reader.ReadInt32();
            Debug.Assert(CompressedBlocksInfoSize >= 0);
            UncompressedBlocksInfoSize = reader.ReadInt32();
            Debug.Assert(UncompressedBlocksInfoSize >= 0);
            Flags = (BundleFlags)reader.ReadInt32();
        }
    }
}
