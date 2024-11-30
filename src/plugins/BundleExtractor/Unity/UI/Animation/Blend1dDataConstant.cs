using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Blend1dDataConstant // wrong labeled
    {
        public float[] m_ChildThresholdArray;

        public Blend1dDataConstant(UIReader reader)
        {
            m_ChildThresholdArray = reader.ReadArray(r => r.ReadSingle());
        }
    }
}
