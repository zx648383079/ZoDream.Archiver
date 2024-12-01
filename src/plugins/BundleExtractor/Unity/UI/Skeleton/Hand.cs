using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Hand
    {
        public int[] m_HandBoneIndex;

        public Hand(IBundleBinaryReader reader)
        {
            m_HandBoneIndex = reader.ReadArray(r => r.ReadInt32());
        }
    }

}
