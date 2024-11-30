using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SkeletonMaskElement
    {
        public uint m_PathHash;
        public float m_Weight;

        public SkeletonMaskElement(UIReader reader)
        {
            m_PathHash = reader.ReadUInt32();
            m_Weight = reader.ReadSingle();
        }
    }
}
