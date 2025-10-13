using System;
using UnityEngine;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class SpriteFontConverter : BundleConverter<SpriteFont>
    {
        public override SpriteFont? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new SpriteFont() 
            {
                Texture = serializer.Deserialize<Texture2D>(reader),
                Glyphs = reader.ReadArray(_ => XnbReader.ReadVector4I(reader)),
                Cropping = reader.ReadArray(_ => XnbReader.ReadVector4I(reader)),
                CharacterMap = reader.ReadArray(_ => XnbReader.ReadChar(reader)),
                VerticalLineSpacing = reader.ReadInt32(),
                HorizontalSpacing = reader.ReadSingle(),
                Kerning = reader.ReadArray(_ => reader.ReadVector3()),
                DefaultCharacter = reader.ReadBoolean() ? XnbReader.ReadChar(reader) : null,
            };
        }
    }
}
