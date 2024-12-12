using System.IO;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocPartKeyFormOffsetPtr
    {
        public uint DrawOrders {  get; private set; }

        public void Read(BinaryReader reader)
        {
            DrawOrders = reader.ReadUInt32();
        }
    }
    internal class MocPartKeyFormOffset
    {
        public float[] DrawOrders { get; private set; }

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocPartKeyFormOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            DrawOrders = reader.ReadArray(ptr.DrawOrders, count, () => reader.ReadSingle());
           

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
