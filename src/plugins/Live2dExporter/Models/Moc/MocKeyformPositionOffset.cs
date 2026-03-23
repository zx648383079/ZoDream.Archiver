using System.IO;
using System.Numerics;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocKeyFormPositionOffsetPtr
    {
        public uint Coords {  get; set; }

        public void Read(BinaryReader reader)
        {
            Coords = reader.ReadUInt32();
        }
    }
    internal class MocKeyFormPositionOffset
    {
        public Vector2[] Coords { get; set; }

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocKeyFormPositionOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            reader.BaseStream.Seek(ptr.Coords, SeekOrigin.Begin);
            Coords = new Vector2[count / 2];
            for (var i = 0; i < count; i+= 2)
            {
                Coords[i / 2] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            }
            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
