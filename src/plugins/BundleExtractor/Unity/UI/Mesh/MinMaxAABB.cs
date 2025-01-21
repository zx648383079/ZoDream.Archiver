using System.Numerics;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class MinMaxAABB
    {
        public Vector3 m_Min;
        public Vector3 m_Max;

        public MinMaxAABB(IBundleBinaryReader reader)
        {
            m_Min = reader.ReadVector3Or4();
            m_Max = reader.ReadVector3Or4();
        }
    }
}
