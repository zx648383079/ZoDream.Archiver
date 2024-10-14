using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.BundleFiles
{
    public class BundleScene
    {
        public uint CompressedSize { get; private set; }
        public uint DecompressedSize { get; private set; }

        public static BundleScene Read(EndianReader reader)
        {
            return new()
            {
                CompressedSize = reader.ReadUInt32(),
                DecompressedSize = reader.ReadUInt32()
            };
        }
    }
}
