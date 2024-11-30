using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SerializedSubShader
    {
        public List<SerializedPass> m_Passes;
        public SerializedTagMap m_Tags;
        public int m_LOD;

        public SerializedSubShader(UIReader reader)
        {
            int numPasses = reader.ReadInt32();
            m_Passes = new List<SerializedPass>();
            for (int i = 0; i < numPasses; i++)
            {
                m_Passes.Add(new SerializedPass(reader));
            }

            m_Tags = new SerializedTagMap(reader);
            m_LOD = reader.ReadInt32();
        }
    }

}
