using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class ControllerConstant
    {
        public List<LayerConstant> m_LayerArray;
        public List<StateMachineConstant> m_StateMachineArray;
        public ValueArrayConstant m_Values;
        public ValueArray m_DefaultValues;

        public ControllerConstant(IBundleBinaryReader reader)
        {
            var scanner = reader.Get<IBundleElementScanner>();
            int numLayers = reader.ReadInt32();
            m_LayerArray = [];
            for (int i = 0; i < numLayers; i++)
            {
                var node = new LayerConstant();
                scanner.TryRead(reader, node);
                m_LayerArray.Add(node);
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
