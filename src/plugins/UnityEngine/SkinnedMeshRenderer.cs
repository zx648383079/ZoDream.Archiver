namespace UnityEngine
{
    public class SkinnedMeshRenderer : Renderer
    {
        public IPPtr<Mesh> Mesh;
        public IPPtr<Transform>[] Bones;
        public float[] BlendShapeWeights;
        public IPPtr<Transform> RootBone;
        public Bounds AABB;
        public bool DirtyAABB;
    }
}
