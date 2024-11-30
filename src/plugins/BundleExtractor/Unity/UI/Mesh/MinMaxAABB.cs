using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class MinMaxAABB
    {
        public Vector3 m_Min;
        public Vector3 m_Max;

        public MinMaxAABB(UIReader reader)
        {
            m_Min = reader.ReadVector3();
            m_Max = reader.ReadVector3();
        }
    }
}
