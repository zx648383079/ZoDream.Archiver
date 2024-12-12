using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocDrawableFlags(byte code)
    {
        public byte BlendMode { get; private set; } = (byte)(code & 0b11);
        public bool IsDoubleSided { get; private set; } = ((code >> 2) & 0x1) != 0;
        public bool IsInverted { get; private set; } = ((code >> 3) & 0x1) != 0;
    }

    internal class MocArtMeshOffsetPtr
    {
        public uint RuntimeSpace0 { get; private set; }
        public uint RuntimeSpace1 { get; private set; }
        public uint RuntimeSpace2 { get; private set; }
        public uint RuntimeSpace3 { get; private set; }
        public uint Ids { get; private set; }
        public uint KeyFormBindingSourcesIndices { get; private set; }
        public uint KeyFormSourcesBeginIndices { get; private set; }
        public uint KeyFormSourcesCounts { get; private set; }
        public uint IsVisible { get; private set; }
        public uint IsEnabled { get; private set; }
        public uint ParentPartIndices { get; private set; }
        public uint ParentDeformerIndices { get; private set; }
        public uint TextureNos { get; private set; }
        public uint DrawableFlags { get; private set; }
        public uint VertexCounts { get; private set; }
        public uint UvSourcesBeginIndices { get; private set; }
        public uint PositionIndexSourcesBeginIndices { get; private set; }
        public uint PositionIndexSourcesCounts { get; private set; }
        public uint DrawableMaskSourcesBeginIndices { get; private set; }
        public uint DrawableMaskSourcesCounts { get; private set; }

        public void Read(BinaryReader reader)
        {
            RuntimeSpace0 = reader.ReadUInt32();
            RuntimeSpace1 = reader.ReadUInt32();
            RuntimeSpace2 = reader.ReadUInt32();
            RuntimeSpace3 = reader.ReadUInt32();
            Ids = reader.ReadUInt32();
            KeyFormBindingSourcesIndices = reader.ReadUInt32();
            KeyFormSourcesBeginIndices = reader.ReadUInt32();
            KeyFormSourcesCounts = reader.ReadUInt32();
            IsVisible = reader.ReadUInt32();
            IsEnabled = reader.ReadUInt32();
            ParentPartIndices = reader.ReadUInt32();
            ParentDeformerIndices = reader.ReadUInt32();
            TextureNos = reader.ReadUInt32();
            DrawableFlags = reader.ReadUInt32();
            VertexCounts = reader.ReadUInt32();
            UvSourcesBeginIndices = reader.ReadUInt32();
            PositionIndexSourcesBeginIndices = reader.ReadUInt32();
            PositionIndexSourcesCounts = reader.ReadUInt32();
            DrawableMaskSourcesBeginIndices = reader.ReadUInt32();
            DrawableMaskSourcesCounts = reader.ReadUInt32();
        }
    }
    internal class MocArtMeshOffset
    {
        public string[] Ids { get; private set; }
        public int[] KeyFormBindingSourcesIndices { get; private set; }
        public int[] KeyFormSourcesBeginIndices { get; private set; }
        public int[] KeyFormSourcesCounts { get; private set; }
        public bool[] IsVisible { get; private set; }
        public bool[] IsEnabled { get; private set; }
        public int[] ParentPartIndices { get; private set; }
        public int[] ParentDeformerIndices { get; private set; }
        public uint[] TextureNos { get; private set; }
        public MocDrawableFlags[] DrawableFlags { get; private set; }
        public int[] VertexCounts { get; private set; }
        public int[] UvSourcesBeginIndices { get; private set; }
        public int[] PositionIndexSourcesBeginIndices { get; private set; }
        public int[] PositionIndexSourcesCounts { get; private set; }
        public int[] DrawableMaskSourcesBeginIndices { get; private set; }
        public int[] DrawableMaskSourcesCounts { get; private set; }

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocArtMeshOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;

            Ids = reader.ReadArray(ptr.Ids, count, () => IOExtension.ConvertString(reader.ReadBytes(64)));

            KeyFormBindingSourcesIndices = reader.ReadArray(ptr.KeyFormBindingSourcesIndices, count, () => reader.ReadInt32());
            KeyFormSourcesBeginIndices = reader.ReadArray(ptr.KeyFormSourcesBeginIndices, count, () => reader.ReadInt32());
            KeyFormSourcesCounts = reader.ReadArray(ptr.KeyFormSourcesCounts, count, () => reader.ReadInt32());

            IsVisible = reader.ReadArray(ptr.IsVisible, count, () => reader.ReadUInt32() > 0);
            IsEnabled = reader.ReadArray(ptr.IsEnabled, count, () => reader.ReadUInt32() > 0);

            ParentPartIndices = reader.ReadArray(ptr.ParentPartIndices, count, () => reader.ReadInt32());
            ParentDeformerIndices = reader.ReadArray(ptr.ParentDeformerIndices, count, () => reader.ReadInt32());
            TextureNos = reader.ReadArray(ptr.TextureNos, count, () => reader.ReadUInt32());
            DrawableFlags = reader.ReadArray(ptr.DrawableFlags, count, () => new MocDrawableFlags(reader.ReadByte()));
            VertexCounts = reader.ReadArray(ptr.VertexCounts, count, () => reader.ReadInt32());

            UvSourcesBeginIndices = reader.ReadArray(ptr.UvSourcesBeginIndices, count, () => reader.ReadInt32());
            PositionIndexSourcesBeginIndices = reader.ReadArray(ptr.PositionIndexSourcesBeginIndices, count, () => reader.ReadInt32());
            PositionIndexSourcesCounts = reader.ReadArray(ptr.PositionIndexSourcesCounts, count, () => reader.ReadInt32());
            DrawableMaskSourcesBeginIndices = reader.ReadArray(ptr.DrawableMaskSourcesBeginIndices, count, () => reader.ReadInt32());
            DrawableMaskSourcesCounts = reader.ReadArray(ptr.DrawableMaskSourcesCounts, count, () => reader.ReadInt32());
           



            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }

    internal class MocArtMeshOffsetV4_2
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
