using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SpriteAtlasDataConverter : BundleConverter<SpriteAtlasData>
    {
        public override SpriteAtlasData? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new SpriteAtlasData
            {
                Texture = serializer.Deserialize<PPtr<Texture2D>>(reader),
                AlphaTexture = serializer.Deserialize<PPtr<Texture2D>>(reader),
                TextureRect = reader.ReadVector4(),
                TextureRectOffset = reader.ReadVector2()
            };
            if (version.GreaterThanOrEquals(2017, 2)) //2017.2 and up
            {
                res.AtlasRectOffset = reader.ReadVector2();
            }
            res.UvTransform = reader.ReadVector4();
            res.DownscaleMultiplier = reader.ReadSingle();
            res.SettingsRaw = serializer.Deserialize<SpriteSettings>(reader);
            if (version.GreaterThanOrEquals(2020, 2)) //2020.2 and up
            {
                res.SecondaryTextures = reader.ReadArray(_ => serializer.Deserialize<SecondarySpriteTexture>(reader));

                reader.AlignStream();
            }
            return res;
        }
    }

}
