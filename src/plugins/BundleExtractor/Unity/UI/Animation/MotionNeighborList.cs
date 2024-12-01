using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class MotionNeighborList
    {
        public uint[] m_NeighborArray;

        public MotionNeighborList(IBundleBinaryReader reader)
        {
            m_NeighborArray = reader.ReadArray(r => r.ReadUInt32());
        }
    }
}
