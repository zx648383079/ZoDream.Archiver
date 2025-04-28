#pragma author zodream
#pragma description Unity Mesh Asset

import zodream.io;

struct UnityMatrix {
    s32 length;
    float value[16 * length];
};

struct StreamingInfo {
    s64 offset;
    u32 size;
    zodream::AlignString path;
};


struct AABB {
    zodream::Vector3 center;
    zodream::Vector3 extent;
};

struct SubMesh {
    u32 firstByte;
    u32 indexCount;
    s32 topology;
    u32 baseVertex;
    u32 firstVertex;
    u32 vertexCount;
    AABB localAABB;
};

struct BlendShapeVertex {
    zodream::Vector3 vertex;
    zodream::Vector3 normal;
    zodream::Vector3 tangent;
    u32 index;
};

struct MeshBlendShape {
    u32 firstVertex;
    u32 vertexCount;
    bool hasNormals;
    bool hasTangents;
};

struct MeshBlendShapeChannel{
    zodream::AlignString name;
    u32 nameHash;
    s32 frameIndex;
    s32 frameCount;
};

struct BlendShapeData {
    zodream::List<BlendShapeVertex> vertices;
    zodream::List<MeshBlendShape> shapes;
    zodream::List<MeshBlendShapeChannel> channels;
    zodream::List<float> fullWeights;
};



struct MinMaxAABB {
    zodream::Vector3 min;
    zodream::Vector3 max;
};
struct VGPackedHierarchyNodeItem {
    zodream::Rect LODBounds;
    zodream::Vector3 BoxBoundsCenter;
    u32 MinLODError_MaxParentLODError;
    zodream::Vector3 BoxBoundsExtent;
    u32 ChildStartReference;
    u32 ResourcePageIndex_NumPages_GroupPartSize;
};
struct VGPackedHierarchyNode {
    VGPackedHierarchyNodeItem items[8];
};

struct VGPageStreamingState {
    u32 BulkOffset;
    u32 BulkSize;
    u32 PageSize;
    u32 DependenciesStart;
    u32 DependenciesNum;
    u32 Flags;
};

struct ChannelInfo {
    u8 stream;
    u8 offset;
    u8 format;
    u8 dimension;
};

struct VertexData {
    u32 VertexCount;
    zodream::List<ChannelInfo> channels;
    zodream::List<u8> DataSize;
};

struct PackedFloatVector {
    u32 NumItems;
    float Range;
    float Start;
    zodream::List<u8> Data;
    zodream::AlignTo<4>;
    u8 BitSize;
    zodream::AlignTo<4>;
};

struct PackedIntVector {
    u32 NumItems;
    zodream::List<u8> Data;
    zodream::AlignTo<4>;
    u8 BitSize;
    zodream::AlignTo<4>;
};

struct CompressedMesh {
    PackedFloatVector Vertices;
    PackedFloatVector UV;
    PackedFloatVector Normals;
    PackedFloatVector Tangents;
    PackedIntVector Weights;
    PackedIntVector NormalSigns;
    PackedIntVector TangentSigns;
    PackedFloatVector FloatColors;
    PackedIntVector BoneIndices;
    PackedIntVector Triangles;
    u32 UVInfo;
};

struct Mesh {
    zodream::AlignString name;
    zodream::List<SubMesh> subItems;
    BlendShapeData shapes;
    UnityMatrix bindPose;
    zodream::List<u32> boneNameHashes;
    u32 rootBoneNameHash;

    zodream::List<MinMaxAABB> bonesAABB;
    zodream::List<u32> variableBoneCountWeights;
    u8 meshCompression;
    bool IsReadable;
    bool KeepVertices;
    bool KeepIndices;
    zodream::AlignTo<4>;
    s32 LightmapUseUV1;
    float fileScale;
    u32 NumInputTriangles;
    u32 NumInputVertices;
    u16 NumInputMeshes;
    u16 NumInputTexCoords;
    u32 ResourceFlags;
    s32 RootClusterPage;
    u32 IndexBuffer[RootClusterPage / 4];
    zodream::List<u16> ImposterAtlas;
    zodream::List<VGPackedHierarchyNode> vgPackedHierarchyNode;
    zodream::List<u32> HierarchyRootOffsets;
    zodream::List<VGPageStreamingState> PageStreamingStates;
    zodream::List<u32> PageDependencies;
    zodream::AlignTo<4>;
    s32;
    if (meshCompression == 0) {
        s32 IndexFormat;
    } else {
        s32 IndexFormat = 0;
    }

    s32 IndexBuffer_size;
    if (IndexFormat == 0) {
        u16 IndexBuffer2[IndexBuffer_size / 2];
    } else {
        u32 IndexBuffer3[IndexBuffer_size / 4];
    }
    zodream::AlignTo<4>;
    VertexData vertexData;
    zodream::AlignTo<4>;
    if (meshCompression > 0) {
        padding[44];
    }
    CompressedMesh compressedMesh;
    
    AABB localAABB;
    s32 MeshUsageFlags;
    s32 CookingOptions;
    zodream::List<u8> BakedConvexCollisionMesh;
    zodream::AlignTo<4>;
    zodream::List<u8> BakedTriangleCollisionMesh;
    zodream::AlignTo<4>;
    float MeshMetrics[2];

    StreamingInfo streamData;
};

Mesh mesh @ 0x0;