using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Bundle;

namespace ZoDream.Live2dExporter.Models
{
    internal enum MocDeformerType: uint
    {
        WARP = 0,
        ROTATION = 1,
    }

    internal class MocDeformerOffsetPtr
    {
        public uint RuntimeSpace0 { get; private set; }
        public uint Ids { get; private set; }
        public uint KeyFormBindingSourcesIndices { get; private set; }
        public uint IsVisible { get; private set; }
        public uint IsEnabled { get; private set; }
        public uint ParentPartIndices { get; private set; }
        public uint ParentDeformerIndices { get; private set; }
        public uint Types { get; private set; }
        public uint SpecificSourcesIndices { get; private set; }

        public void Read(IBundleBinaryReader reader)
        {
            RuntimeSpace0 = reader.ReadUInt32();
            Ids = reader.ReadUInt32();
            KeyFormBindingSourcesIndices = reader.ReadUInt32();
            IsVisible = reader.ReadUInt32();
            IsEnabled = reader.ReadUInt32();
            ParentPartIndices = reader.ReadUInt32();
            ParentDeformerIndices = reader.ReadUInt32();
            Types = reader.ReadUInt32();
            SpecificSourcesIndices = reader.ReadUInt32();
        }
    }
    internal class MocDeformerOffset
    {
        public string[] Ids { get; private set; }
        public int[] KeyFormBindingSourcesIndices { get; private set; }
        public bool[] IsVisible { get; private set; }
        public bool[] IsEnabled { get; private set; }
        public int[] ParentPartIndices { get; private set; }
        public int[] ParentDeformerIndices { get; private set; }
        public MocDeformerType[] Types { get; private set; }
        public int[] SpecificSourcesIndices { get; private set; }

        public void Read(IBundleBinaryReader reader, int count)
        {
            var ptr = new MocDeformerOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;
            Ids = reader.ReadArray(ptr.Ids, count, () => IOExtension.ConvertString(reader.ReadBytes(64)));
            KeyFormBindingSourcesIndices = reader.ReadArray(ptr.KeyFormBindingSourcesIndices, count
                , () => reader.ReadInt32());

            IsVisible = reader.ReadArray(ptr.IsVisible, count, () => reader.ReadUInt32() > 0);
            IsEnabled = reader.ReadArray(ptr.IsEnabled, count, () => reader.ReadUInt32() > 0);
            ParentPartIndices = reader.ReadArray(ptr.ParentPartIndices, count
                , () => reader.ReadInt32());
            ParentDeformerIndices = reader.ReadArray(ptr.ParentDeformerIndices, count
                , () => reader.ReadInt32());

            Types = reader.ReadArray(ptr.Types, count
                , () => (MocDeformerType)reader.ReadUInt32());

            SpecificSourcesIndices = reader.ReadArray(ptr.SpecificSourcesIndices, count
                , () => reader.ReadInt32());

            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
