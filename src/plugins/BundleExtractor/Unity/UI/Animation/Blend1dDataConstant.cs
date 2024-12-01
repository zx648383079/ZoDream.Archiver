using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Blend1dDataConstant // wrong labeled
    {
        public float[] m_ChildThresholdArray;

        public Blend1dDataConstant(IBundleBinaryReader reader)
        {
            m_ChildThresholdArray = reader.ReadArray(r => r.ReadSingle());
        }
    }
}
