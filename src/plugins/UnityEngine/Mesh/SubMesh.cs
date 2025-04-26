
namespace UnityEngine
{
    public class SubMesh
    {
        public uint FirstByte;
        public uint IndexCount;
        public GfxPrimitiveType Topology;
        public uint TriangleCount;
        public uint BaseVertex;
        public uint FirstVertex;
        public uint VertexCount;
        public Bounds LocalAABB;

    }

}
