using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class ValueDelta
    {
        public float m_Start;
        public float m_Stop;

        public ValueDelta(IBundleBinaryReader reader)
        {
            m_Start = reader.ReadSingle();
            m_Stop = reader.ReadSingle();
        }
    }
}
