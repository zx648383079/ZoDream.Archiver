using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Bundle;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocBlendShapeParameterBindingOffsetPtr
    {
        public uint KeysSourcesBeginIndices { get; private set; }
        public uint KeysSourcesCounts { get; private set; }
        public uint BaseKeyIndices { get; private set; }

        public void Read(IBundleBinaryReader reader)
        {
            KeysSourcesBeginIndices = reader.ReadUInt32();
            KeysSourcesCounts = reader.ReadUInt32();
            BaseKeyIndices = reader.ReadUInt32();
        }
    }
    internal class MocBlendShapeParameterBindingOffset
    {
        public int[] KeysSourcesBeginIndices { get; private set; }
        public int[] KeysSourcesCounts { get; private set; }
        public int[] BaseKeyIndices { get; private set; }

        public void Read(IBundleBinaryReader reader, int count)
        {
            var ptr = new MocBlendShapeParameterBindingOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            KeysSourcesBeginIndices = reader.ReadArray(ptr.KeysSourcesBeginIndices, count, () => reader.ReadInt32());
            KeysSourcesCounts = reader.ReadArray(ptr.KeysSourcesCounts, count, () => reader.ReadInt32());
            BaseKeyIndices = reader.ReadArray(ptr.BaseKeyIndices, count, () => reader.ReadInt32());

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
