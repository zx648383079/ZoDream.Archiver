using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.WebFiles
{
    public sealed class WebFileEntry : ReadOnlyEntry
    {
        public WebFileEntry(string name, long length) : base(name, length)
        {
        }

        public static WebFileEntry Read(EndianReader reader)
        {
            return new(reader.ReadString(), reader.ReadInt32())
            {
                Offset = reader.ReadInt32(),
            };
        }


        public int Offset { get; private set; }
    }
}
