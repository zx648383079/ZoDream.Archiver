using System.IO;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocPartOffsetPtr
    {
        public uint RuntimeSpace0 { get; private set; }
        public uint Ids { get; private set; }
        public uint KeyFormBindingSourcesIndices { get; private set; }
        public uint KeyFormSourcesBeginIndices { get; private set; }
        public uint KeyFormSourcesCounts { get; private set; }
        public uint IsVisible { get; private set; }
        public uint IsEnabled { get; private set; }
        public uint ParentPartIndices { get; private set; }

        public void Read(BinaryReader reader)
        {
            RuntimeSpace0 = reader.ReadUInt32();
            Ids = reader.ReadUInt32();
            KeyFormBindingSourcesIndices = reader.ReadUInt32();
            KeyFormSourcesBeginIndices = reader.ReadUInt32();
            KeyFormSourcesCounts = reader.ReadUInt32();
            IsVisible = reader.ReadUInt32();
            IsEnabled = reader.ReadUInt32();
            ParentPartIndices = reader.ReadUInt32();
        }
    }
    internal class MocPartOffset
    {
        public string[] Ids { get; private set; }
        public int[] KeyFormBindingSourcesIndices { get; private set; }
        public int[] KeyFormSourcesBeginIndices { get; private set; }
        public int[] KeyFormSourcesCounts { get; private set; }
        public bool[] IsVisible { get; private set; }
        public bool[] IsEnabled { get; private set; }
        public int[] ParentPartIndices { get; private set; }

        public void Read(BinaryReader reader, int count)
        {
            var ptr = new MocPartOffsetPtr();
            ptr.Read(reader);
            var pos = reader.BaseStream.Position;
            Ids = reader.ReadArray(ptr.Ids, count, () => IOExtension.ConvertString(reader.ReadBytes(64)));
            KeyFormBindingSourcesIndices = reader.ReadArray(ptr.KeyFormBindingSourcesIndices, count, () => reader.ReadInt32());
            KeyFormSourcesBeginIndices = reader.ReadArray(ptr.KeyFormSourcesBeginIndices, count, () => reader.ReadInt32());
            KeyFormSourcesCounts = reader.ReadArray(ptr.KeyFormSourcesCounts, count, () => reader.ReadInt32());
            IsVisible = reader.ReadArray(ptr.IsVisible, count, () => reader.ReadUInt32() > 0);
            IsEnabled = reader.ReadArray(ptr.IsEnabled, count, () => reader.ReadUInt32() > 0);
            ParentPartIndices = reader.ReadArray(ptr.ParentPartIndices, count, () => reader.ReadInt32());


            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
