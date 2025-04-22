using System.Numerics;

namespace UnityEngine
{
    public class SpriteRenderData
    {
        public IPPtr<Texture2D> Texture;
        public IPPtr<Texture2D> AlphaTexture;
        public SecondarySpriteTexture[] SecondaryTextures;
        public SubMesh[] SubMeshes;
        public byte[] IndexBuffer;
        public VertexData VertexData;
        public SpriteVertex[] Vertices;
        public ushort[] Indices;
        public Matrix4x4[] Bindpose;
        public BoneWeights4[] SourceSkin = [];
        public Vector4 TextureRect;
        public Vector2 TextureRectOffset;
        public Vector2 AtlasRectOffset;
        public SpriteSettings SettingsRaw;
        public Vector4 UvTransform;
        public float DownscaleMultiplier;

    }

}
