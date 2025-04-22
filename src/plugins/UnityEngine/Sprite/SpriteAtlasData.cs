using System.Numerics;

namespace UnityEngine
{
    public class SpriteAtlasData
    {
        public IPPtr<Texture2D> Texture;
        public IPPtr<Texture2D> AlphaTexture;
        public Vector4 TextureRect;
        public Vector2 TextureRectOffset;
        public Vector2 AtlasRectOffset;
        public Vector4 UvTransform;
        public float DownscaleMultiplier;
        public SpriteSettings SettingsRaw;
        public SecondarySpriteTexture[] SecondaryTextures;

    }

}
