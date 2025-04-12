using System.Collections.Generic;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class SkinnedMeshRenderer(UIReader reader) 
        : UIRenderer(reader)
    {
        public PPtr<Mesh> m_Mesh;
        public List<PPtr<Transform>> m_Bones;
        public float[] m_BlendShapeWeights;
        public PPtr<Transform> m_RootBone;
        public AABB m_AABB;
        public bool m_DirtyAABB;


        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            ReadBase2(reader);
        }
        public void ReadBase2(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();
            int m_Quality = reader.ReadInt32();
            var m_UpdateWhenOffscreen = reader.ReadBoolean();
            var m_SkinNormals = reader.ReadBoolean(); //3.1.0 and below
            reader.AlignStream();

            if (version.LessThan(2, 6)) //2.6 down
            {
                var m_DisableAnimationWhenOffscreen = new PPtr<Animation>(reader);
            }

            m_Mesh = new PPtr<Mesh>(reader);

            var numBones = reader.ReadInt32();
            m_Bones = [];
            for (int b = 0; b < numBones; b++)
            {
                m_Bones.Add(new PPtr<Transform>(reader));
            }

            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                m_BlendShapeWeights = reader.ReadArray(r => r.ReadSingle());
            }

            
        }

        public override void Associated(IDependencyBuilder? builder)
        {
            base.Associated(builder);
            builder?.AddDependencyEntry(_reader.FullPath, FileID, m_Mesh.m_PathID);
            builder?.AddDependencyEntry(_reader.FullPath, FileID, m_RootBone.m_PathID);
            foreach (var item in m_Bones)
            {
                builder?.AddDependencyEntry(_reader.FullPath, FileID, item.m_PathID);
            }
        }
    }
}
