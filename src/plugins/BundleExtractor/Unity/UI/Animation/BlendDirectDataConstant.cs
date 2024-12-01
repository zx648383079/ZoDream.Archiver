using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class BlendDirectDataConstant
    {
        public uint[] m_ChildBlendEventIDArray;
        public bool m_NormalizedBlendValues;

        public BlendDirectDataConstant(IBundleBinaryReader reader)
        {
            m_ChildBlendEventIDArray = reader.ReadArray(r => r.ReadUInt32());
            m_NormalizedBlendValues = reader.ReadBoolean();
            reader.AlignStream();
        }
    }
}
