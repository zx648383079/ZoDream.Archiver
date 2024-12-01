using System.Collections.Generic;
using System.Numerics;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Blend2dDataConstant
    {
        public Vector2[] m_ChildPositionArray;
        public float[] m_ChildMagnitudeArray;
        public Vector2[] m_ChildPairVectorArray;
        public float[] m_ChildPairAvgMagInvArray;
        public List<MotionNeighborList> m_ChildNeighborListArray;

        public Blend2dDataConstant(IBundleBinaryReader reader)
        {
            m_ChildPositionArray = reader.ReadArray(_ => reader.ReadVector2());
            m_ChildMagnitudeArray = reader.ReadArray(r => r.ReadSingle());
            m_ChildPairVectorArray = reader.ReadArray(_ => reader.ReadVector2());
            m_ChildPairAvgMagInvArray = reader.ReadArray(r => r.ReadSingle());

            int numNeighbours = reader.ReadInt32();
            m_ChildNeighborListArray = new List<MotionNeighborList>();
            for (int i = 0; i < numNeighbours; i++)
            {
                m_ChildNeighborListArray.Add(new MotionNeighborList(reader));
            }
        }
    }
}
