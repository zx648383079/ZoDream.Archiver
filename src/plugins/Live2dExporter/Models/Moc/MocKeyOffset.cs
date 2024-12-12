using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocKeyOffsetPtr
    {
        public uint Values { get; private set; }

        public void Read(BinaryReader reader)
        {
            Values = reader.ReadUInt32();
        }
    }
    internal class MocKeyOffset
    {
        public float[] Values { get; private set; }

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocKeyOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            Values = reader.ReadArray(ptr.Values, count, () => reader.ReadSingle());
           

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
