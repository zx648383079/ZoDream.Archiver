using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SerializedProperties
    {
        public List<SerializedProperty> m_Props;

        public SerializedProperties(IBundleBinaryReader reader)
        {
            int numProps = reader.ReadInt32();
            m_Props = [];
            for (int i = 0; i < numProps; i++)
            {
                m_Props.Add(new SerializedProperty(reader));
            }
        }
    }

}
