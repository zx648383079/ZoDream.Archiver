using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SkeletonPose
    {
        public XForm<Vector3>[] m_X;

        public SkeletonPose(UIReader reader)
        {
            m_X = reader.ReadXFormArray();
        }
    }

}
