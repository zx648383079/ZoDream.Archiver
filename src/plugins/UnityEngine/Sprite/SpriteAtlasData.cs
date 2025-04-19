using System.Numerics;

namespace UnityEngine
{
    public class SpriteAtlasData
    {
        public PPtr<Texture2D> Texture;
        public PPtr<Texture2D> AlphaTexture;
        public Vector4 TextureRect;
        public Vector2 TextureRectOffset;
        public Vector2 AtlasRectOffset;
        public Vector4 UvTransform;
        public float DownscaleMultiplier;
        public SpriteSettings SettingsRaw;
        public SecondarySpriteTexture[] SecondaryTextures;

    }

}
