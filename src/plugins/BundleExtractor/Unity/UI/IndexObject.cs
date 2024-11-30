using System.Collections.Generic;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Index
    {
        public PPtr<UIObject> Object;
        public ulong Size;

        public Index(UIReader reader)
        {
            Object = new PPtr<UIObject>(reader);
            Size = reader.ReadUInt64();
        }
    }

    internal sealed class IndexObject : NamedObject
    {
        public int Count;
        public List<KeyValuePair<string, Index>> AssetMap;

        public override string Name => "IndexObject";

        public IndexObject(UIReader reader) : base(reader)
        {
            Count = reader.ReadInt32();
            AssetMap = new List<KeyValuePair<string, Index>>();
            for (int i = 0; i < Count; i++)
            {
                AssetMap.Add(new KeyValuePair<string, Index>(reader.ReadAlignedString(), new Index(reader)));
            }
        }
    }
}
