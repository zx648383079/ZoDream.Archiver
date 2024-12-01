using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SelectorTransitionConstant
    {
        public uint m_Destination;
        public List<ConditionConstant> m_ConditionConstantArray;

        public SelectorTransitionConstant(IBundleBinaryReader reader)
        {
            m_Destination = reader.ReadUInt32();

            int numConditions = reader.ReadInt32();
            m_ConditionConstantArray = [];
            for (int i = 0; i < numConditions; i++)
            {
                m_ConditionConstantArray.Add(new ConditionConstant(reader));
            }
        }
    }
}
