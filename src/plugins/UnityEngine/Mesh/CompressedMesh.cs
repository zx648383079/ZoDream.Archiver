
namespace UnityEngine
{
    public class CompressedMesh
    {
        public PackedFloatVector Vertices;
        public PackedFloatVector UV;
        public PackedFloatVector BindPoses;
        public PackedFloatVector Normals;
        public PackedFloatVector Tangents;
        public PackedIntVector Weights;
        public PackedIntVector NormalSigns;
        public PackedIntVector TangentSigns;
        public PackedFloatVector FloatColors;
        public PackedIntVector BoneIndices;
        public PackedIntVector Triangles;
        public PackedIntVector Colors;
        public uint UVInfo;

    }

}
