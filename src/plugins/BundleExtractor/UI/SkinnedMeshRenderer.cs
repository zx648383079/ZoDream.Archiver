using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZoDream.BundleExtractor.UI
{
    public sealed class SkinnedMeshRenderer : UIRenderer
    {
        public PPtr<Mesh> m_Mesh;
        public List<PPtr<Transform>> m_Bones;
        public float[] m_BlendShapeWeights;
        public PPtr<Transform> m_RootBone;
        public AABB m_AABB;
        public bool m_DirtyAABB;

        public SkinnedMeshRenderer(UIReader reader) : base(reader)
        {
            var version = reader.Version;
            int m_Quality = reader.Reader.ReadInt32();
            var m_UpdateWhenOffscreen = reader.Reader.ReadBoolean();
            var m_SkinNormals = reader.Reader.ReadBoolean(); //3.1.0 and below
            reader.Reader.AlignStream();

            if (version.LessThan(2,6)) //2.6 down
            {
                var m_DisableAnimationWhenOffscreen = new PPtr<Animation>(reader);
            }

            m_Mesh = new PPtr<Mesh>(reader);

            var numBones = reader.Reader.ReadInt32();
            m_Bones = new List<PPtr<Transform>>();
            for (int b = 0; b < numBones; b++)
            {
                m_Bones.Add(new PPtr<Transform>(reader));
            }

            if (version.GreaterThanOrEquals(4,3)) //4.3 and up
            {
                m_BlendShapeWeights = reader.Reader.ReadArray(r => r.ReadSingle());
            }

            if (reader.IsGIGroup())
            {
                m_RootBone = new PPtr<Transform>(reader);
                m_AABB = new AABB(reader);
                m_DirtyAABB = reader.Reader.ReadBoolean();
                reader.Reader.AlignStream();
            }
        }
    }
}
