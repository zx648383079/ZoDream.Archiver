using System.Collections.Generic;

namespace ZoDream.BundleExtractor.Unity.UI
{
    public class ResourceManager : UIObject
    {
        public List<KeyValuePair<string, PPtr<UIObject>>> m_Container;

        public ResourceManager(UIReader reader) : base(reader)
        {
            var m_ContainerSize = reader.Reader.ReadInt32();
            m_Container = new List<KeyValuePair<string, PPtr<UIObject>>>();
            for (int i = 0; i < m_ContainerSize; i++)
            {
                m_Container.Add(new KeyValuePair<string, PPtr<UIObject>>(reader.ReadAlignedString(), new PPtr<UIObject>(reader)));
            }
        }
    }
}
