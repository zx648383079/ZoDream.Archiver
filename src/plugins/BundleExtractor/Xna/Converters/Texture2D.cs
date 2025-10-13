using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class Texture2DConverter : BundleConverter<Texture2D>
    {
        public override Texture2D? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new Texture2D()
            {
                TextureFormat = reader.ReadInt32() switch
                {
                    4 => TextureFormat.DXT1,
                    5 => TextureFormat.DXT3,
                    6 => TextureFormat.DXT5,
                    _ => TextureFormat.RGBA32
                },
                Width = reader.ReadInt32(),
                Height = reader.ReadInt32(),
                MipCount = reader.ReadInt32(),
                ImageData = reader.ReadAsStream()
            };
        }
    }
}
