using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SelectorTransitionConstant
    {
        public uint m_Destination;
        public List<ConditionConstant> m_ConditionConstantArray;

        public SelectorTransitionConstant(UIReader reader)
        {
            m_Destination = reader.ReadUInt32();

            int numConditions = reader.ReadInt32();
            m_ConditionConstantArray = new List<ConditionConstant>();
            for (int i = 0; i < numConditions; i++)
            {
                m_ConditionConstantArray.Add(new ConditionConstant(reader));
            }
        }
    }
}
