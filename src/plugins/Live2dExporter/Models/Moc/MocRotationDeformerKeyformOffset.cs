using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocRotationDeformerKeyFormOffsetPtr
    {
        public uint Opacities { get; private set; }
        public uint Angles { get; private set; }
        public uint OriginX { get; private set; }
        public uint OriginY { get; private set; }
        public uint Scales { get; private set; }
        public uint IsReflectX { get; private set; }
        public uint IsReflectY { get; private set; }

        public void Read(BinaryReader reader)
        {
            Opacities = reader.ReadUInt32();
            Angles = reader.ReadUInt32();
            OriginX = reader.ReadUInt32();
            OriginY = reader.ReadUInt32();
            Scales = reader.ReadUInt32();
            IsReflectX = reader.ReadUInt32();
            IsReflectY = reader.ReadUInt32();
        }
    }
    internal class MocRotationDeformerKeyFormOffset
    {
        public float[] Opacities { get; private set; }
        public float[] Angles { get; private set; }
        public float[] OriginX { get; private set; }
        public float[] OriginY { get; private set; }
        public float[] Scales { get; private set; }
        public bool[] IsReflectX { get; private set; }
        public bool[] IsReflectY { get; private set; }

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocRotationDeformerKeyFormOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            Opacities = reader.ReadArray(ptr.Opacities, count, () => reader.ReadSingle());
            Angles = reader.ReadArray(ptr.Angles, count, () => reader.ReadSingle());
            OriginX = reader.ReadArray(ptr.OriginX, count, () => reader.ReadSingle());
            OriginY = reader.ReadArray(ptr.OriginY, count, () => reader.ReadSingle());
            Scales = reader.ReadArray(ptr.Scales, count, () => reader.ReadSingle());
            IsReflectX = reader.ReadArray(ptr.IsReflectX, count, () => reader.ReadUInt32() > 0);
            IsReflectY = reader.ReadArray(ptr.IsReflectY, count, () => reader.ReadUInt32() > 0);


            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }

    internal class MocRotationDeformerKeyFormOffsetsV5_0
    {
        public int[] KeyFormMultiplyColorSourcesBeginIndices { get; private set; }
        public int[] KeyFormScreenColorSourcesBeginIndices { get; private set; }
        public void Read(BinaryReader reader, int count)
        {
            var ptr = reader.ReadUInt32();
            var ptr2 = reader.ReadUInt32();
            var pos = reader.BaseStream.Position;

            KeyFormMultiplyColorSourcesBeginIndices = reader.ReadArray(ptr, count, () => reader.ReadInt32());
            KeyFormScreenColorSourcesBeginIndices = reader.ReadArray(ptr2, count, () => reader.ReadInt32());


            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
