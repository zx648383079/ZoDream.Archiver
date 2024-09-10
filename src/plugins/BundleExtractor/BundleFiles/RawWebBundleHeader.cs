using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.BundleFiles
{
    public abstract class RawWebBundleHeader: BundleHeader
    {
        public byte[] Hash { get; set; } = new byte[16];
        public uint Crc { get; set; }
        /// <summary>
        /// Minimum number of bytes to read for streamed bundles, equal to BundleSize for normal bundles
        /// </summary>
        public uint MinimumStreamedBytes { get; set; }
        public int HeaderSize { get; set; }
        /// <summary>
        /// Equal to 1 if it's a streamed bundle, number of LZMAChunkInfos + mainData assets otherwise
        /// </summary>
        public int NumberOfScenesToDownloadBeforeStreaming { get; set; }
        /// <summary>
        /// LZMA chunks info
        /// </summary>
        public BundleScene[] Scenes { get; set; } = [];
        public uint CompleteFileSize { get; set; }
        public int UncompressedBlocksInfoSize { get; set; }

        public sealed override void Read(EndianReader reader)
        {
            base.Read(reader);
            if (HasHash(Version))
            {
                reader.Read(Hash, 0, Hash.Length);
                Crc = reader.ReadUInt32();
            }
            MinimumStreamedBytes = reader.ReadUInt32();
            HeaderSize = reader.ReadInt32();
            NumberOfScenesToDownloadBeforeStreaming = reader.ReadInt32();
            Scenes = reader.ReadArray(BundleScene.Read);
            if (HasCompleteFileSize(Version))
            {
                CompleteFileSize = reader.ReadUInt32();
            }
            if (HasUncompressedBlocksInfoSize(Version))
            {
                UncompressedBlocksInfoSize = (int)reader.ReadUInt32();
            }
            reader.AlignStream();
        }

        /// <summary>
		/// 5.2.0 and greater / Bundle Version 4 +
		/// </summary>
		public static bool HasHash(BundleVersion generation) => generation >= BundleVersion.BF_520a1;
        /// <summary>
        /// 2.6.0 and greater / Bundle Version 2 +
        /// </summary>
        public static bool HasCompleteFileSize(BundleVersion generation) => generation >= BundleVersion.BF_260_340;
        /// <summary>
        /// 3.5.0 and greater / Bundle Version 3 +
        /// </summary>
        public static bool HasUncompressedBlocksInfoSize(BundleVersion generation) => generation >= BundleVersion.BF_350_4x;
    }
}
