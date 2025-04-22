namespace UnityEngine
{
    public abstract class Renderer : Component
    {
        public IPPtr<Material>[] Materials;
        public StaticBatchInfo StaticBatchInfo;
        public uint[] SubsetIndices;
    }

    public struct StaticBatchInfo
    {
        public ushort FirstSubMesh;
        public ushort SubMeshCount;
    }
}