using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class MotionNeighborList
    {
        public uint[] m_NeighborArray;

        public MotionNeighborList(UIReader reader)
        {
            m_NeighborArray = reader.ReadArray(r => r.ReadUInt32());
        }
    }
}
