using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SkeletonMask
    {
        public List<SkeletonMaskElement> m_Data;

        public SkeletonMask(UIReader reader)
        {
            int numElements = reader.ReadInt32();
            m_Data = new List<SkeletonMaskElement>();
            for (int i = 0; i < numElements; i++)
            {
                m_Data.Add(new SkeletonMaskElement(reader));
            }
        }
    }
}
