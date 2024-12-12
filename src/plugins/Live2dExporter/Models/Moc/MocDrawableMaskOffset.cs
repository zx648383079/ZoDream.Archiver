using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocDrawableMaskOffsetPtr
    {
        public uint ArtMeshSourcesIndices { get; private set; }

        public void Read(BinaryReader reader)
        {
            ArtMeshSourcesIndices = reader.ReadUInt32();
        }
    }
    internal class MocDrawableMaskOffset
    {
        public int[] ArtMeshSourcesIndices { get; private set; }

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocDrawableMaskOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            ArtMeshSourcesIndices = reader.ReadArray(ptr.ArtMeshSourcesIndices, count, () => reader.ReadInt32());
            

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
