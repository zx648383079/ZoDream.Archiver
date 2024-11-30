using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class LeafInfoConstant
    {
        public uint[] m_IDArray;
        public uint m_IndexOffset;

        public LeafInfoConstant(UIReader reader)
        {
            m_IDArray = reader.ReadArray(r => r.ReadUInt32());
            m_IndexOffset = reader.ReadUInt32();
        }
    }
}
