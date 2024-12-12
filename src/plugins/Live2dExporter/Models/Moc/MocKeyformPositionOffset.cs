using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocKeyFormPositionOffsetPtr
    {
        public uint Xys {  get; set; }

        public void Read(BinaryReader reader)
        {
            Xys = reader.ReadUInt32();
        }
    }
    internal class MocKeyFormPositionOffset
    {
        public Vector2[] Xys { get; set; }

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocKeyFormPositionOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            reader.BaseStream.Seek(ptr.Xys, SeekOrigin.Begin);
            Xys = new Vector2[count / 2];
            for (var i = 0; i < count; i+= 2)
            {
                Xys[i / 2] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            }
            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
