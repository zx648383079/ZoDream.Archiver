using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocKeyFormColorOffsetPtr
    {
        public uint R {  get; private set; }
        public uint G {  get; private set; }
        public uint B {  get; private set; }

        public void Read(BinaryReader reader)
        {
            R = reader.ReadUInt32();
            G = reader.ReadUInt32();
            B = reader.ReadUInt32();
        }
    }
    internal class MocKeyFormColorOffset
    {
        public float[] R { get; private set; }
        public float[] G { get; private set; }
        public float[] B { get; private set; }

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocKeyFormColorOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            R = reader.ReadArray(ptr.R, count, () => reader.ReadSingle());
            G = reader.ReadArray(ptr.G, count, () => reader.ReadSingle());
            B = reader.ReadArray(ptr.B, count, () => reader.ReadSingle());

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }

    }
}
