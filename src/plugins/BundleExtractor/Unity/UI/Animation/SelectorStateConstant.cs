using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SelectorStateConstant
    {
        public List<SelectorTransitionConstant> m_TransitionConstantArray;
        public uint m_FullPathID;
        public bool m_isEntry;

        public SelectorStateConstant(UIReader reader)
        {
            int numTransitions = reader.ReadInt32();
            m_TransitionConstantArray = new List<SelectorTransitionConstant>();
            for (int i = 0; i < numTransitions; i++)
            {
                m_TransitionConstantArray.Add(new SelectorTransitionConstant(reader));
            }

            m_FullPathID = reader.ReadUInt32();
            m_isEntry = reader.ReadBoolean();
            reader.AlignStream();
        }
    }

}
