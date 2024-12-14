using System.IO;
using ZoDream.Shared.Bundle;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocParameterBindingIndicesOffsetPtr
    {
        public uint BindingSourcesIndices {  get; private set; }

        public void Read(IBundleBinaryReader reader)
        {
            BindingSourcesIndices = reader.ReadUInt32();
        }
    }
    internal class MocParameterBindingIndicesOffset
    {
        public int[] BindingSourcesIndices { get; private set; }

        public void Read(IBundleBinaryReader reader, int count)
        {
            var ptr = new MocParameterBindingIndicesOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            BindingSourcesIndices = reader.ReadArray(ptr.BindingSourcesIndices, count, () => reader.ReadInt32());
            
            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
