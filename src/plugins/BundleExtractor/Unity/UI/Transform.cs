using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace ZoDream.BundleExtractor.Unity.UI
{
    public class Transform : UIComponent
    {
        public Vector4 m_LocalRotation;
        public Vector3 m_LocalPosition;
        public Vector3 m_LocalScale;
        public List<PPtr<Transform>> m_Children;
        public PPtr<Transform> m_Father;

        public Transform(UIReader reader) : base(reader)
        {
            m_LocalRotation = reader.ReadVector4();
            m_LocalPosition = reader.ReadVector3();
            m_LocalScale = reader.ReadVector3();

            int m_ChildrenCount = reader.Reader.ReadInt32();
            m_Children = new List<PPtr<Transform>>();
            for (int i = 0; i < m_ChildrenCount; i++)
            {
                m_Children.Add(new PPtr<Transform>(reader));
            }
            m_Father = new PPtr<Transform>(reader);
        }
    }
}
