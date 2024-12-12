using System.IO;

namespace ZoDream.Live2dExporter.Models
{

    internal class MocArtMeshKeyFormOffsetPtr
    {
        public uint Opacities { get; private set; }
        public uint DrawOrders { get; private set; }
        public uint KeyFormPositionSourcesBeginIndices { get; private set; }

        public void Read(BinaryReader reader)
        {
            Opacities = reader.ReadUInt32();
            DrawOrders = reader.ReadUInt32();
            KeyFormPositionSourcesBeginIndices = reader.ReadUInt32();
        }
    }

    internal class MocArtMeshKeyFormOffset
    {
        public float[] Opacities { get; set; }
        public float[] DrawOrders { get; set; }
        public int[] KeyFormPositionSourcesBeginIndices { get; set; }
        

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocArtMeshKeyFormOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;


            Opacities = reader.ReadArray(ptr.Opacities, count, () => reader.ReadSingle());
            

            DrawOrders = reader.ReadArray(ptr.DrawOrders, count, () => reader.ReadSingle());

            KeyFormPositionSourcesBeginIndices = reader.ReadArray(ptr.KeyFormPositionSourcesBeginIndices, 
                count, () => reader.ReadInt32());


            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }

    internal class MocArtMeshKeyFormOffsetsV5_0
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
