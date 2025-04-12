using System.Collections.Generic;
using System.Numerics;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Transform(UIReader reader) : UIComponent(reader)
    {
        public Quaternion m_LocalRotation;
        public Vector3 m_LocalPosition;
        public Vector3 m_LocalScale;
        public List<PPtr<Transform>> m_Children;
        public PPtr<Transform> m_Father;

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            m_LocalRotation = reader.ReadQuaternion();
            m_LocalPosition = reader.ReadVector3Or4();
            m_LocalScale = reader.ReadVector3Or4();

            int m_ChildrenCount = reader.ReadInt32();
            m_Children = [];
            for (int i = 0; i < m_ChildrenCount; i++)
            {
                m_Children.Add(new PPtr<Transform>(reader));
            }
            m_Father = new PPtr<Transform>(reader);
        }

        public override void Associated(IDependencyBuilder? builder)
        {
            base.Associated(builder);
            builder?.AddDependencyEntry(_reader.FullPath, FileID, m_Father.m_PathID);
            foreach (var child in m_Children)
            {
                builder?.AddDependencyEntry(_reader.FullPath, FileID, child.m_PathID);
            }
        }
    }
}
