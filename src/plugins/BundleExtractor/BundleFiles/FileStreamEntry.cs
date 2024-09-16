using System;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.BundleFiles
{
    public class FileStreamEntry(string name, long length): ReadOnlyEntry(name, length)
    {
        public long Offset { get; set; }

        public NodeFlags Flags { get; set; }

        public static FileStreamEntry Read(EndianReader reader)
        {
            var offset = reader.ReadInt64();
            var size = reader.ReadInt64();
            var flags = (NodeFlags)reader.ReadInt32();
            var path = reader.ReadStringZeroTerm();
            return new(path, size)
            {
                Offset = offset,
                Flags = flags,
            };
        }
    }

    [Flags]
    public enum NodeFlags
    {
        Default = 0,
        Directory = 1,
        Deleted = 2,
        SerializedFile = 4,
    }
}
