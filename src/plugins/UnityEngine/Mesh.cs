using System.Numerics;

namespace UnityEngine
{
    public sealed class Mesh : Object
    {

        public int IndexFormat;
        public bool Use16BitIndices => IndexFormat == 0;

        public MeshLodInfo MeshLodInfo { get; set; }

        public SubMesh[] SubMeshes;
        public byte[]? IndexBuffer;
        public uint[] IndexBufferFormat;
        public BlendShapeData Shapes;
        public Matrix4x4[] BindPose;
        public uint[] BoneNameHashes;
        public int VertexCount;
        public float[] Vertices;
        public BoneWeights4[] Skin;
        public float[] Normals;
        public float[] Colors;
        public float[] UV0;
        public float[] UV1;
        public float[] UV2;
        public float[] UV3;
        public float[] UV4;
        public float[] UV5;
        public float[] UV6;
        public float[] UV7;
        public float[] Tangents;
        public VertexData VertexData;
        public CompressedMesh CompressedMesh;
        public ResourceSource StreamData;
        public bool CollisionMeshBaked = false;

        public uint[] Indices = [];
    }
}
