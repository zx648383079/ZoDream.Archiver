using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class ResourceManager(UIReader reader) : UIObject(reader)
    {
        public List<KeyValuePair<string, PPtr<UIObject>>> m_Container;


        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            var m_ContainerSize = reader.ReadInt32();
            m_Container = [];
            for (int i = 0; i < m_ContainerSize; i++)
            {
                m_Container.Add(new KeyValuePair<string, PPtr<UIObject>>(reader.ReadAlignedString(), new PPtr<UIObject>(reader)));
            }
        }
    }
}
