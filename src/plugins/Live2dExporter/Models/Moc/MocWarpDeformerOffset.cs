using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocWarpDeformerOffsetPtr
    {
        public uint KeyFormBindingSourcesIndices { get; private set; }
        public uint KeyFormSourcesBeginIndices { get; private set; }
        public uint KeyFormSourcesCounts { get; private set; }
        public uint VertexCounts { get; private set; }
        public uint Rows { get; private set; }
        public uint Columns { get; private set; }

        public void Read(BinaryReader reader)
        {
            KeyFormBindingSourcesIndices = reader.ReadUInt32();
            KeyFormSourcesBeginIndices = reader.ReadUInt32();
            KeyFormSourcesCounts = reader.ReadUInt32();
            VertexCounts = reader.ReadUInt32();
            Rows = reader.ReadUInt32();
            Columns = reader.ReadUInt32();
        }
    }
    internal class MocWarpDeformerOffset
    {
        public int[] KeyFormBindingSourcesIndices { get; private set; }
        public int[] KeyFormSourcesBeginIndices { get; private set; }
        public int[] KeyFormSourcesCounts { get; private set; }
        public int[] VertexCounts { get; private set; }
        public uint[] Rows { get; private set; }
        public uint[] Columns { get; private set; }

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocWarpDeformerOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            KeyFormBindingSourcesIndices = reader.ReadArray(ptr.KeyFormBindingSourcesIndices, count, () => reader.ReadInt32());
            KeyFormSourcesBeginIndices = reader.ReadArray(ptr.KeyFormSourcesBeginIndices, count, () => reader.ReadInt32());
            KeyFormSourcesCounts = reader.ReadArray(ptr.KeyFormSourcesCounts, count, () => reader.ReadInt32());
            VertexCounts = reader.ReadArray(ptr.VertexCounts, count, () => reader.ReadInt32());
            Rows = reader.ReadArray(ptr.Rows, count, () => reader.ReadUInt32());
            Columns = reader.ReadArray(ptr.Columns, count, () => reader.ReadUInt32());
            

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
