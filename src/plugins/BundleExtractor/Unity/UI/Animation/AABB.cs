using System;
using System.Numerics;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class AABB
    {
        public Vector3 m_Center;
        public Vector3 m_Extent;

        public AABB(IBundleBinaryReader reader)
        {
            m_Center = reader.ReadVector3Or4();
            m_Extent = reader.ReadVector3Or4();
        }

    }

}
