using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Hand
    {
        public int[] m_HandBoneIndex;

        public Hand(UIReader reader)
        {
            m_HandBoneIndex = reader.ReadArray(r => r.ReadInt32());
        }
    }

}
