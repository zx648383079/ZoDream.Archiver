using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class LeafInfoConstant
    {
        public uint[] m_IDArray;
        public uint m_IndexOffset;

        public LeafInfoConstant(IBundleBinaryReader reader)
        {
            m_IDArray = reader.ReadArray(r => r.ReadUInt32());
            m_IndexOffset = reader.ReadUInt32();
        }
    }
}
