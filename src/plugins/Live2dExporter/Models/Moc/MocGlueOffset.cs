using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocGlueOffsetPtr
    {
        public uint RuntimeSpace0 { get; private set; }
        public uint Ids { get; private set; }
        public uint KeyFormBindingSourcesIndices { get; private set; }
        public uint KeyFormSourcesBeginIndices { get; private set; }
        public uint KeyFormSourcesCounts { get; private set; }
        public uint ArtMeshIndicesA { get; private set; }
        public uint ArtMeshIndicesB { get; private set; }
        public uint GlueInfoSourcesBeginIndices { get; private set; }
        public uint GlueInfoSourcesCounts { get; private set; }


        public void Read(BinaryReader reader)
        {
            RuntimeSpace0 = reader.ReadUInt32();
            Ids = reader.ReadUInt32();
            KeyFormBindingSourcesIndices = reader.ReadUInt32();
            KeyFormSourcesBeginIndices = reader.ReadUInt32();
            KeyFormSourcesCounts = reader.ReadUInt32();
            ArtMeshIndicesA = reader.ReadUInt32();
            ArtMeshIndicesB = reader.ReadUInt32();
            GlueInfoSourcesBeginIndices = reader.ReadUInt32();
            GlueInfoSourcesCounts = reader.ReadUInt32();
        }

    }
    internal class MocGlueOffset
    {
        public string[] Ids { get; private set; }
        public int[] KeyFormBindingSourcesIndices { get; private set; }
        public int[] KeyFormSourcesBeginIndices { get; private set; }
        public int[] KeyFormSourcesCounts { get; private set; }
        public int[] ArtMeshIndicesA { get; private set; }
        public int[] ArtMeshIndicesB { get; private set; }
        public int[] GlueInfoSourcesBeginIndices { get; private set; }
        public int[] GlueInfoSourcesCounts { get; private set; }

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocGlueOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;
            Ids = reader.ReadArray(ptr.Ids, count, () => IOExtension.ConvertString(reader.ReadBytes(64)));
            KeyFormBindingSourcesIndices = reader.ReadArray(ptr.KeyFormBindingSourcesIndices, count, () => reader.ReadInt32());
            KeyFormSourcesBeginIndices = reader.ReadArray(ptr.KeyFormSourcesBeginIndices, count, () => reader.ReadInt32());
            KeyFormSourcesCounts = reader.ReadArray(ptr.KeyFormSourcesCounts, count, () => reader.ReadInt32());
            ArtMeshIndicesA = reader.ReadArray(ptr.ArtMeshIndicesA, count, () => reader.ReadInt32());
            ArtMeshIndicesB = reader.ReadArray(ptr.ArtMeshIndicesB, count, () => reader.ReadInt32());
            GlueInfoSourcesBeginIndices = reader.ReadArray(ptr.GlueInfoSourcesBeginIndices, count, () => reader.ReadInt32());
            GlueInfoSourcesCounts = reader.ReadArray(ptr.GlueInfoSourcesCounts, count, () => reader.ReadInt32());
            

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
