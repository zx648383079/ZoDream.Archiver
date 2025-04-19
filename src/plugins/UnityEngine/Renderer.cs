namespace UnityEngine
{
    public abstract class Renderer : Component
    {
        public PPtr<Material>[] Materials;
        public StaticBatchInfo StaticBatchInfo;
        public uint[] SubsetIndices;
    }

    public struct StaticBatchInfo
    {
        public ushort FirstSubMesh;
        public ushort SubMeshCount;
    }
}