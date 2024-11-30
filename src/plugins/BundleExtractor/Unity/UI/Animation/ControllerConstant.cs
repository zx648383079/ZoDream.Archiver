using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class ControllerConstant
    {
        public List<LayerConstant> m_LayerArray;
        public List<StateMachineConstant> m_StateMachineArray;
        public ValueArrayConstant m_Values;
        public ValueArray m_DefaultValues;

        public ControllerConstant(UIReader reader)
        {
            int numLayers = reader.ReadInt32();
            m_LayerArray = new List<LayerConstant>();
            for (int i = 0; i < numLayers; i++)
            {
                m_LayerArray.Add(new LayerConstant(reader));
            }

            int numStates = reader.ReadInt32();
            m_StateMachineArray = new List<StateMachineConstant>();
            for (int i = 0; i < numStates; i++)
            {
                m_StateMachineArray.Add(new StateMachineConstant(reader));
            }

            m_Values = new ValueArrayConstant(reader);
            m_DefaultValues = new ValueArray(reader);
        }
    }
}
