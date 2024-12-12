using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocGlueKeyFormOffsetPtr
    {
        public uint Intensities { get; private set; }

        public void Read(BinaryReader reader)
        {
            Intensities = reader.ReadUInt32();
        }
    }
    internal class MocGlueKeyFormOffset
    {
        public float[] Intensities { get; private set; }
        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocGlueKeyFormOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            Intensities = reader.ReadArray(ptr.Intensities, count, () => reader.ReadSingle());
            
            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
