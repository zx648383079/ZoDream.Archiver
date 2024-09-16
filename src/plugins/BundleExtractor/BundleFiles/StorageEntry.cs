using System;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.BundleFiles
{
    public class StorageEntry(long length, long compressedLength) : 
        SplitStreamEntry(length, compressedLength)
    {
        public StorageEntryFlags Flags { get; private set; }

        public override CompressionType CompressionType => (CompressionType)(Flags & StorageEntryFlags.CompressionTypeMask);

        public static StorageEntry Read(EndianReader reader)
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
