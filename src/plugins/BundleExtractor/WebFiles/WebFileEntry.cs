using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.WebFiles
{
    public sealed class WebFileEntry(string name, long length) : ReadOnlyEntry(name, length)
    {
        public int Offset { get; private set; }
        public static WebFileEntry Read(EndianReader reader)
        {
            return new(reader.ReadString(), reader.ReadInt32())
            {
                Offset = reader.ReadInt32(),
            };
        }


      
    }
}
