using System.Collections.Generic;

namespace ZoDream.BundleExtractor.Unity.UI
{
    public class AssetInfo
    {
        public int preloadIndex;
        public int preloadSize;
        public PPtr<UIObject> asset;

        public AssetInfo(UIReader reader)
        {
            preloadIndex = reader.Reader.ReadInt32();
            preloadSize = reader.Reader.ReadInt32();
            asset = new PPtr<UIObject>(reader);
        }
    }

    public sealed class AssetBundle : NamedObject
    {
        public List<PPtr<UIObject>> m_PreloadTable;
        public List<KeyValuePair<string, AssetInfo>> m_Container;

        public AssetBundle(UIReader reader) : base(reader)
        {
            var m_PreloadTableSize = reader.Reader.ReadInt32();
            m_PreloadTable = new List<PPtr<UIObject>>();
            for (int i = 0; i < m_PreloadTableSize; i++)
            {
                m_PreloadTable.Add(new PPtr<UIObject>(reader));
            }

            var m_ContainerSize = reader.Reader.ReadInt32();
            m_Container = new List<KeyValuePair<string, AssetInfo>>();
            for (int i = 0; i < m_ContainerSize; i++)
            {
                m_Container.Add(new KeyValuePair<string, AssetInfo>(reader.ReadAlignedString(), new AssetInfo(reader)));
            }
        }
    }
}
