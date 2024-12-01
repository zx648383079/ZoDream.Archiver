using System.Numerics;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SkeletonPose
    {
        public XForm<Vector3>[] m_X;

        public SkeletonPose(IBundleBinaryReader reader)
        {
            m_X = reader.ReadXFormArray();
        }
    }

}
