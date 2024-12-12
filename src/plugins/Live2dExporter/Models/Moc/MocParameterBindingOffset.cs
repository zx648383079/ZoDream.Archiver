using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocParameterBindingOffsetPtr
    {
        public uint KeysSourcesBeginIndices {  get; private set; }
        public uint KeysSourcesCounts {  get; private set; }

        public void Read(BinaryReader reader)
        {
            KeysSourcesBeginIndices = reader.ReadUInt32();
            KeysSourcesCounts = reader.ReadUInt32();
        }

    }
    internal class MocParameterBindingOffset
    {
        public int[] KeysSourcesBeginIndices { get; private set; }
        public int[] KeysSourcesCounts { get; private set; }
        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocParameterBindingOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            KeysSourcesBeginIndices = reader.ReadArray(ptr.KeysSourcesBeginIndices, count, () => reader.ReadInt32());
            KeysSourcesCounts = reader.ReadArray(ptr.KeysSourcesCounts, count, () => reader.ReadInt32());
           

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
