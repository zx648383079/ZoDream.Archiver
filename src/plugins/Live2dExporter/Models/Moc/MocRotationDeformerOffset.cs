using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocRotationDeformerOffsetPtr
    {
        public uint KeyFormBindingSourcesIndices { get; private set; }
        public uint KeyFormSourcesBeginIndices { get; private set; }
        public uint KeyFormSourcesCounts { get; private set; }
        public uint BaseAngles { get; private set; }

        public void Read(BinaryReader reader)
        {
            KeyFormBindingSourcesIndices = reader.ReadUInt32();
            KeyFormSourcesBeginIndices = reader.ReadUInt32();
            KeyFormSourcesCounts = reader.ReadUInt32();
            BaseAngles = reader.ReadUInt32();
        }
    }
    internal class MocRotationDeformerOffset
    {
        public int[] KeyFormBindingSourcesIndices { get; private set; }
        public int[] KeyFormSourcesBeginIndices { get; private set; }
        public int[] KeyFormSourcesCounts { get; private set; }
        public float[] BaseAngles { get; private set; }

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocRotationDeformerOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            KeyFormBindingSourcesIndices = reader.ReadArray(ptr.KeyFormBindingSourcesIndices, count, () => reader.ReadInt32());
            KeyFormSourcesBeginIndices = reader.ReadArray(ptr.KeyFormSourcesBeginIndices, count, () => reader.ReadInt32());
            KeyFormSourcesCounts = reader.ReadArray(ptr.KeyFormSourcesCounts, count, () => reader.ReadInt32());
            BaseAngles = reader.ReadArray(ptr.BaseAngles, count, () => reader.ReadSingle());


            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }

    internal class MocRotationDeformerOffsetV4_2
    {
        public int[] keyformColorSourcesBeginIndices { get; private set; }
        public void Read(BinaryReader reader, int count)
        {
            var ptr = reader.ReadUInt32();
            var pos = reader.BaseStream.Position;

            keyformColorSourcesBeginIndices = reader.ReadArray(ptr, count, () => reader.ReadInt32());


            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
