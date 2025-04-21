namespace UnityEngine
{
    public class SkinnedMeshRenderer : Renderer
    {
        public PPtr<Mesh> Mesh;
        public PPtr<Transform>[] Bones;
        public float[] BlendShapeWeights;
        public PPtr<Transform> RootBone;
        public Bounds AABB;
        public bool DirtyAABB;
    }
}
