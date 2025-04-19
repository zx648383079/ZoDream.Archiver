using System.Collections.Generic;

namespace UnityEngine
{
    public class SkinnedMeshRenderer : Renderer
    {
        public PPtr<Mesh> Mesh;
        public List<PPtr<Transform>> Bones;
        public float[] BlendShapeWeights;
        public PPtr<Transform> RootBone;
        public Bounds AABB;
        public bool DirtyAABB;
    }
}
