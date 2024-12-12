using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocParameterBindingIndicesOffsetPtr
    {
        public uint BindingSourcesIndices {  get; private set; }

        public void Read(BinaryReader reader)
        {
            BindingSourcesIndices = reader.ReadUInt32();
        }
    }
    internal class MocParameterBindingIndicesOffset
    {
        public int[] BindingSourcesIndices { get; private set; }

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocParameterBindingIndicesOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            BindingSourcesIndices = reader.ReadArray(ptr.BindingSourcesIndices, count, () => reader.ReadInt32());
            
            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
