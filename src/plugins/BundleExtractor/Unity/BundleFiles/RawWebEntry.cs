using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.BundleFiles
{
    public class RawWebEntry(string name, long length) : ReadOnlyEntry(name, length)
    {
        public long Offset { get; set; }


        public static RawWebEntry Read(IBundleBinaryReader reader)
        {
            var path = reader.ReadStringZeroTerm();
            var offset = reader.ReadInt32();
            var size = reader.ReadInt64();
            return new(path, size)
            {
                Offset = offset,
            };
        }
    }
}
