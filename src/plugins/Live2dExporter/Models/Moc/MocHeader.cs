using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocHeader
    {
        public string Signature { get; set; }
        public MocVersion Version { get; set; }
        public bool IsBigEndian { get; set; }

        public bool TryRead(Stream input)
        {
            input.Seek(0, SeekOrigin.Begin);
            var buffer = new byte[4];
            input.ReadExactly(buffer);
            Signature = Encoding.ASCII.GetString(buffer);
            if (Signature != "MOC3")
            {
                return false;
            }
            Version = (MocVersion)input.ReadByte();
            IsBigEndian = input.ReadByte() == 1;
            input.Seek(58, SeekOrigin.Current);
            return true;
        }
    }
}
