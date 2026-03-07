using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class Texture2DConverter : BundleConverter<Texture2D>
    {
        public override Texture2D? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new Texture2D()
            {
                TextureFormat = ToFormat(reader.ReadInt32()),
                Width = reader.ReadInt32(),
                Height = reader.ReadInt32(),
                MipCount = reader.ReadInt32(),
            };
            res.ImageData = new MultipartFileStream(reader.ReadArray(res.MipCount, _ => reader.ReadAsStream()));
            return res;
        }

        public static TextureFormat ToFormat(int val)
        {
            return val switch
            {
                4 => TextureFormat.DXT1,
                5 => TextureFormat.DXT3,
                6 => TextureFormat.DXT5,
                _ => TextureFormat.RGBA32
            };
        }
    }

    internal class Texture3DConverter : BundleConverter<Texture3D>
    {
        public override Texture3D? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new Texture3D()
            {
                TextureFormat = Texture2DConverter.ToFormat(reader.ReadInt32()),
                Width = reader.ReadInt32(),
                Height = reader.ReadInt32(),
                Depth = reader.ReadInt32(),
                MipCount = reader.ReadInt32(),
            };
            res.ImageData = new MultipartFileStream(reader.ReadArray(res.MipCount, _ => reader.ReadAsStream()));
            return res;
        }
    }
}
