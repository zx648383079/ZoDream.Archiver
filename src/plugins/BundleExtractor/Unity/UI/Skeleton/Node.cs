using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Node
    {
        public int m_ParentId;
        public int m_AxesId;

        public Node(UIReader reader)
        {
            m_ParentId = reader.ReadInt32();
            m_AxesId = reader.ReadInt32();
        }
    }
}
