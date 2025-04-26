using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SecondarySpriteTextureConverter : BundleConverter<SecondarySpriteTexture>
    {
        public override SecondarySpriteTexture? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                Texture = reader.ReadPPtr<Texture2D>(serializer),
                Name = reader.ReadStringZeroTerm(),
            };
        }
    }
}
