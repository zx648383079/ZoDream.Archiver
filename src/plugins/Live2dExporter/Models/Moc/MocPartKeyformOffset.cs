using System.IO;
using ZoDream.Shared.Bundle;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocPartKeyFormOffsetPtr
    {
        public uint DrawOrders {  get; private set; }

        public void Read(IBundleBinaryReader reader)
        {
            DrawOrders = reader.ReadUInt32();
        }
    }
    internal class MocPartKeyFormOffset
    {
        public float[] DrawOrders { get; private set; }

        public void Read(IBundleBinaryReader reader, int count)
        {
            var ptr = new MocPartKeyFormOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            DrawOrders = reader.ReadArray(ptr.DrawOrders, count, () => reader.ReadSingle());
           

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
