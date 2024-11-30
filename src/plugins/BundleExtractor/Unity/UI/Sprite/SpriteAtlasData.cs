using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SpriteAtlasData
    {
        public PPtr<Texture2D> texture;
        public PPtr<Texture2D> alphaTexture;
        public Rectf textureRect;
        public Vector2 textureRectOffset;
        public Vector2 atlasRectOffset;
        public Vector4 uvTransform;
        public float downscaleMultiplier;
        public SpriteSettings settingsRaw;
        public List<SecondarySpriteTexture> secondaryTextures;

        public SpriteAtlasData(UIReader reader)
        {
            var version = reader.Version;
            texture = new PPtr<Texture2D>(reader);
            alphaTexture = new PPtr<Texture2D>(reader);
            textureRect = new Rectf(reader);
            textureRectOffset = reader.ReadVector2();
            if (version.GreaterThanOrEquals(2017, 2)) //2017.2 and up
            {
                atlasRectOffset = reader.ReadVector2();
            }
            uvTransform = reader.ReadVector4();
            downscaleMultiplier = reader.ReadSingle();
            settingsRaw = new SpriteSettings(reader);
            if (version.GreaterThanOrEquals(2020, 2)) //2020.2 and up
            {
                var secondaryTexturesSize = reader.ReadInt32();
                secondaryTextures = [];
                for (int i = 0; i < secondaryTexturesSize; i++)
                {
                    secondaryTextures.Add(new SecondarySpriteTexture(reader));
                }
                reader.AlignStream();
            }
        }
    }

}
