using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal enum MocDrawOrderGroupObjectType : uint
    {
        ART_MESH = 0,
        PART = 1,
    }
    internal class MocDrawOrderGroupObjectOffsetPtr
    {
        public uint Types { get; private set; }
        public uint Indices { get; private set; }
        public uint SelfIndices { get; private set; }

        public void Read(BinaryReader reader)
        {
            Types = reader.ReadUInt32();
            Indices = reader.ReadUInt32();
            SelfIndices = reader.ReadUInt32();
        }
    }
    internal class MocDrawOrderGroupObjectOffset
    {
        public MocDrawOrderGroupObjectType[] Types { get; private set; }
        public int[] Indices { get; private set; }
        public int[] SelfIndices { get; private set; }

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocDrawOrderGroupObjectOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            Types = reader.ReadArray(ptr.Types, count, () => (MocDrawOrderGroupObjectType)reader.ReadUInt32());
            Indices = reader.ReadArray(ptr.Indices, count, () => reader.ReadInt32());
            SelfIndices = reader.ReadArray(ptr.SelfIndices, count, () => reader.ReadInt32());
            

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
