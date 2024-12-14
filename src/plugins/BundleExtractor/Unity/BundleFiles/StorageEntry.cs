using System;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.BundleFiles
{
    public class StorageEntry(long length, long compressedLength) :
        SplitStreamEntry(length, compressedLength)
    {
        public StorageEntryFlags Flags { get; private set; }

        public override BundleCodecType CodecType => ((UnityCompressionType)(Flags & StorageEntryFlags.CompressionTypeMask)).ToCodec();

        public static StorageEntry Read(IBundleBinaryReader reader)
        {
            return new(reader.ReadUInt32(), reader.ReadUInt32())
            {
                Flags = (StorageEntryFlags)reader.ReadUInt16()
            };
        }
    }

    [Flags]
    public enum StorageEntryFlags
    {
        CompressionTypeMask = 0x3F,

        Streamed = 0x40,
    }
}
