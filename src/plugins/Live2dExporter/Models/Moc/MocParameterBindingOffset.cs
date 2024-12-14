using System.IO;
using ZoDream.Shared.Bundle;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocParameterBindingOffsetPtr
    {
        public uint KeysSourcesBeginIndices {  get; private set; }
        public uint KeysSourcesCounts {  get; private set; }

        public void Read(IBundleBinaryReader reader)
        {
            KeysSourcesBeginIndices = reader.ReadUInt32();
            KeysSourcesCounts = reader.ReadUInt32();
        }

    }
    internal class MocParameterBindingOffset
    {
        public int[] KeysSourcesBeginIndices { get; private set; }
        public int[] KeysSourcesCounts { get; private set; }
        public void Read(IBundleBinaryReader reader, int count)
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
