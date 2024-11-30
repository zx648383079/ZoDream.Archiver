using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class StateMachineConstant
    {
        public List<StateConstant> m_StateConstantArray;
        public List<TransitionConstant> m_AnyStateTransitionConstantArray;
        public List<SelectorStateConstant> m_SelectorStateConstantArray;
        public uint m_DefaultState;
        public uint m_MotionSetCount;

        public StateMachineConstant(UIReader reader)
        {
            var version = reader.Version;

            int numStates = reader.ReadInt32();
            m_StateConstantArray = new List<StateConstant>();
            for (int i = 0; i < numStates; i++)
            {
                m_StateConstantArray.Add(new StateConstant(reader));
            }

            int numAnyStates = reader.ReadInt32();
            m_AnyStateTransitionConstantArray = new List<TransitionConstant>();
            for (int i = 0; i < numAnyStates; i++)
            {
                m_AnyStateTransitionConstantArray.Add(new TransitionConstant(reader));
            }

            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                int numSelectors = reader.ReadInt32();
                m_SelectorStateConstantArray = new List<SelectorStateConstant>();
                for (int i = 0; i < numSelectors; i++)
                {
                    m_SelectorStateConstantArray.Add(new SelectorStateConstant(reader));
                }
            }

            m_DefaultState = reader.ReadUInt32();
            m_MotionSetCount = reader.ReadUInt32();
        }
    }

}
