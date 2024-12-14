using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Bundle;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocWarpDeformerKeyFormOffsetPtr
    {
        public uint Opacities {  get; private set; }
        public uint KeyFormPositionSourcesBeginIndices {  get; private set; }

        public void Read(IBundleBinaryReader reader)
        {
            Opacities = reader.ReadUInt32();
            KeyFormPositionSourcesBeginIndices = reader.ReadUInt32();
        }
    }
    internal class MocWarpDeformerKeyFormOffset
    {
        public float[] Opacities { get; private set; }
        public int[] KeyFormPositionSourcesBeginIndices { get; private set; }
        public void Read(IBundleBinaryReader reader, int count)
        {
            var ptr = new MocWarpDeformerKeyFormOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            Opacities = reader.ReadArray(ptr.Opacities, count, () => reader.ReadSingle());
            KeyFormPositionSourcesBeginIndices = reader.ReadArray(ptr.KeyFormPositionSourcesBeginIndices, count, () => reader.ReadInt32());
           

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }

    internal class MocWarpDeformerKeyFormOffsetV3_3
    {
        public bool[] IsQuadSource { get; private set; }
        public void Read(IBundleBinaryReader reader, int count)
        {
            var ptr = reader.ReadUInt32();
            var pos = reader.BaseStream.Position;

            IsQuadSource = reader.ReadArray(ptr, count, () => reader.ReadUInt32() > 0);
            

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }

    internal class MocWarpDeformerKeyFormOffsetV4_2
    {
        public int[] KeyFormColorSourcesBeginIndices { get; private set; }
        public void Read(IBundleBinaryReader reader, int count)
        {
            var ptr = reader.ReadUInt32();
            var pos = reader.BaseStream.Position;

            KeyFormColorSourcesBeginIndices = reader.ReadArray(ptr, count, () => reader.ReadInt32());


            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }

    internal class MocWarpDeformerKeyFormOffsetV5_0
    {
        public int[] KeyFormMultiplyColorSourcesBeginIndices { get; private set; }
        public int[] KeyFormScreenColorSourcesBeginIndices { get; private set; }
        public void Read(IBundleBinaryReader reader, int count)
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
