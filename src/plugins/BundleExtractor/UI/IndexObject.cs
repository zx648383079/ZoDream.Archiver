using System.Collections.Generic;
using ZoDream.BundleExtractor.SerializedFiles;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.UI
{
    public class Index
    {
        public PPtr<UIObject> Object;
        public ulong Size;

        public Index(UIReader reader)
        {
            Object = new PPtr<UIObject>(reader);
            Size = reader.Reader.ReadUInt64();
        }
    }

    public sealed class IndexObject : NamedObject
    {
        public int Count;
        public List<KeyValuePair<string, Index>> AssetMap;

        public override string Name => "IndexObject";

        public IndexObject(UIReader reader) : base(reader)
        {
            Count = reader.Reader.ReadInt32();
            AssetMap = new List<KeyValuePair<string, Index>>();
            for (int i = 0; i < Count; i++)
            {
                AssetMap.Add(new KeyValuePair<string, Index>(reader.ReadAlignedString(), new Index(reader)));
            }
        }
    } 
}
