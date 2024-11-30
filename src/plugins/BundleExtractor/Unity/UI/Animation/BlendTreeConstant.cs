using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class BlendTreeConstant
    {
        public List<BlendTreeNodeConstant> m_NodeArray;
        public ValueArrayConstant m_BlendEventArrayConstant;

        public BlendTreeConstant(UIReader reader)
        {
            var version = reader.Version;

            int numNodes = reader.ReadInt32();
            m_NodeArray = new List<BlendTreeNodeConstant>();
            for (int i = 0; i < numNodes; i++)
            {
                m_NodeArray.Add(new BlendTreeNodeConstant(reader));
            }

            if (version.LessThan(4, 5)) //4.5 down
            {
                m_BlendEventArrayConstant = new ValueArrayConstant(reader);
            }
        }
    }
}
