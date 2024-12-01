using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Transform(UIReader reader) : UIComponent(reader)
    {
        public Vector4 m_LocalRotation;
        public Vector3 m_LocalPosition;
        public Vector3 m_LocalScale;
        public List<PPtr<Transform>> m_Children;
        public PPtr<Transform> m_Father;

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            m_LocalRotation = reader.ReadVector4();
            m_LocalPosition = reader.ReadVector3();
            m_LocalScale = reader.ReadVector3();

            int m_ChildrenCount = reader.ReadInt32();
            m_Children = [];
            for (int i = 0; i < m_ChildrenCount; i++)
            {
                m_Children.Add(new PPtr<Transform>(reader));
            }
            m_Father = new PPtr<Transform>(reader);
        }
    }
}
