using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocUVOffsetPtr
    {
        public uint Uvs { get; set; }
        public void Read(BinaryReader reader)
        {
            Uvs = reader.ReadUInt32();
        }
    }
    internal class MocUVOffset
    {
        public Vector2[] Uvs { get; set; }

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocUVOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            reader.BaseStream.Seek(ptr.Uvs, SeekOrigin.Begin);
            Uvs = new Vector2[count / 2];
            for (var i = 0; i < count; i += 2)
            {
                Uvs[i / 2] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            }
            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
