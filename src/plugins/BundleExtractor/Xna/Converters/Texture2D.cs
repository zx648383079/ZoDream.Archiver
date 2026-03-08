using System;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class Texture2DConverter : BundleConverter<Texture2D>, IBundleConvertExporter<Texture2D>
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
        public void SaveAs(object instance, string fileName, ArchiveExtractMode mode)
        {
            if (instance is Texture2D o)
            {
                SaveAs(o, fileName, mode);
            }
        }


        public void SaveAs(Texture2D instance, string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".png", mode, out fileName))
            {
                return;
            }
            var source = instance.ImageData is MultipartFileStream o ? o[0] : instance.ImageData;
            var data = TextureExtension.Decode(source, instance.Width,
                        instance.Height, instance.TextureFormat, UnityEngine.Version.MinVersion);
            if (data is null)
            {
                return;
            }
            using var image = data.ToImage();
            image?.SaveAs(fileName);
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
