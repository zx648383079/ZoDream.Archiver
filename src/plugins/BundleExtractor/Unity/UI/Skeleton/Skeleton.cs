using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Skeleton
    {
        public List<Node> m_Node;
        public uint[] m_ID;
        public List<Axes> m_AxesArray;

        public Skeleton(UIReader reader)
        {
            int numNodes = reader.ReadInt32();
            m_Node = new List<Node>();
            for (int i = 0; i < numNodes; i++)
            {
                m_Node.Add(new Node(reader));
            }

            m_ID = reader.ReadArray(r => r.ReadUInt32());

            int numAxes = reader.ReadInt32();
            m_AxesArray = new List<Axes>();
            for (int i = 0; i < numAxes; i++)
            {
                m_AxesArray.Add(new Axes(reader));
            }
        }
    }
}
