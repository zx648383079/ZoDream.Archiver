using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SkeletonMask
    {
        public List<SkeletonMaskElement> m_Data;

        public SkeletonMask(IBundleBinaryReader reader)
        {
            int numElements = reader.ReadInt32();
            m_Data = [];
            for (int i = 0; i < numElements; i++)
            {
                m_Data.Add(new SkeletonMaskElement(reader));
            }
        }
    }
}
