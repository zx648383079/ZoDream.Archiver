using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SerializedProperties
    {
        public List<SerializedProperty> m_Props;

        public SerializedProperties(UIReader reader)
        {
            int numProps = reader.ReadInt32();
            m_Props = new List<SerializedProperty>();
            for (int i = 0; i < numProps; i++)
            {
                m_Props.Add(new SerializedProperty(reader));
            }
        }
    }

}
