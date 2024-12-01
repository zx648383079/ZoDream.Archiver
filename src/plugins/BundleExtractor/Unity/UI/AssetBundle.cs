using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class AssetInfo
    {
        public int preloadIndex;
        public int preloadSize;
        public PPtr<UIObject> asset;

        public AssetInfo(IBundleBinaryReader reader)
        {
            preloadIndex = reader.ReadInt32();
            preloadSize = reader.ReadInt32();
            asset = new PPtr<UIObject>(reader);
        }
    }

    internal sealed class AssetBundle(UIReader reader) : NamedObject(reader)
    {
        public List<PPtr<UIObject>> m_PreloadTable;
        public List<KeyValuePair<string, AssetInfo>> m_Container;

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            var m_PreloadTableSize = reader.ReadInt32();
            m_PreloadTable = [];
            for (int i = 0; i < m_PreloadTableSize; i++)
            {
                m_PreloadTable.Add(new PPtr<UIObject>(reader));
            }

            var m_ContainerSize = reader.ReadInt32();

            m_Container = [];
            for (int i = 0; i < m_ContainerSize; i++)
            {
                m_Container.Add(new KeyValuePair<string, AssetInfo>(reader.ReadAlignedString(), new AssetInfo(reader)));
            }
        }
    }
}
