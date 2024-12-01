using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Index
    {
        public PPtr<UIObject> Object;
        public ulong Size;

        public Index(IBundleBinaryReader reader)
        {
            Object = new PPtr<UIObject>(reader);
            Size = reader.ReadUInt64();
        }
    }

    internal sealed class IndexObject(UIReader reader) : NamedObject(reader)
    {
        public int Count;
        public List<KeyValuePair<string, Index>> AssetMap;

        public override string Name => "IndexObject";

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            Count = reader.ReadInt32();
            AssetMap = [];
            for (int i = 0; i < Count; i++)
            {
                AssetMap.Add(new KeyValuePair<string, Index>(reader.ReadAlignedString(), new Index(reader)));
            }
        }
    }
}
