using System.Collections.Generic;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class BlendTreeConstant
    {
        public List<BlendTreeNodeConstant> m_NodeArray;
        public ValueArrayConstant m_BlendEventArrayConstant;

        public BlendTreeConstant(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();
            var scanner = reader.Get<IBundleElementScanner>();
            int numNodes = reader.ReadInt32();
            m_NodeArray = [];
            for (int i = 0; i < numNodes; i++)
            {
                var node = new BlendTreeNodeConstant();
                scanner.TryRead(reader, node);
                m_NodeArray.Add(node);
            }

            if (version.LessThan(4, 5)) //4.5 down
            {
                m_BlendEventArrayConstant = new ValueArrayConstant(reader);
            }
        }
    }
}
