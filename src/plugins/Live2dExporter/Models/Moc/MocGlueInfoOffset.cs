using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocGlueInfoOffsetPtr
    {
        public uint Weights { get; private set; }
        public uint PositionIndices { get; private set; }

        public void Read(BinaryReader reader)
        {
            Weights = reader.ReadUInt32();
            PositionIndices = reader.ReadUInt32();
        }
    }
    internal class MocGlueInfoOffset
    {
        public float[] Weights { get; private set; }
        public short[] PositionIndices { get; private set; }

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocGlueInfoOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            Weights = reader.ReadArray(ptr.Weights, count, () => reader.ReadSingle());
            PositionIndices = reader.ReadArray(ptr.PositionIndices, count, () => reader.ReadInt16());
           

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
