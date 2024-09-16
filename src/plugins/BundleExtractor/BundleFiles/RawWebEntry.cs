using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.BundleFiles
{
    public class RawWebEntry(string name, long length) : ReadOnlyEntry(name, length)
    {
        public long Offset { get; set; }


        public static RawWebEntry Read(EndianReader reader)
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
