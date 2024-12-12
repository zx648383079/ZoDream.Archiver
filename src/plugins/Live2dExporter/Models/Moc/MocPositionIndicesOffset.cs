using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocPositionIndicesOffsetPtr
    {
        public uint Indices { get; private set; }

        public void Read(BinaryReader reader)
        {
            Indices = reader.ReadUInt32();
        }
    }
    internal class MocPositionIndicesOffset
    {
        public short[] Indices { get; private set; }

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocPositionIndicesOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            Indices = reader.ReadArray(ptr.Indices, count, () => reader.ReadInt16());
            

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
