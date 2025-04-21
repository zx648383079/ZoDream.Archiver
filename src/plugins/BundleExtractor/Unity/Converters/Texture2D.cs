using SkiaSharp;
using System;
using System.Buffers;
using System.IO;
using UnityEngine;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    

    internal class GLTextureSettingsConverter : BundleConverter<GLTextureSettings>
    {
        public override GLTextureSettings Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new GLTextureSettings
            {
                FilterMode = reader.ReadInt32(),
                Aniso = reader.ReadInt32(),
                MipBias = reader.ReadSingle()
            };
            if (version.Major >= 2017)//2017.x and up
            {
                res.WrapMode = reader.ReadInt32(); //m_WrapU
                res.WrapV = reader.ReadInt32();
                res.WrapW = reader.ReadInt32();
            }
            else
            {
                res.WrapMode = reader.ReadInt32();
            }
            return res;
        }
    }

    internal sealed class Texture2DConverter : BundleConverter<Texture2D>, IBundleExporter, IElementTypeLoader
    {
        public Texture2D? Read(IBundleBinaryReader reader, TypeTree typeMaps)
        {
            var res = new Texture2D();
            TypeTreeHelper.ReadType(typeMaps, reader, res);
            if (!string.IsNullOrEmpty(res.StreamData.Source))
            {
                var container = reader.Get<ISerializedFile>();
                if (reader.TryGet<IDependencyBuilder>(out var builder))
                {
                    var fileName = container.FullPath;
                    var fileId = reader.Get<ObjectInfo>().FileID;
                    builder.AddDependencyEntry(fileName,
                        fileId,
                        res.StreamData.Source);
                }
                res.ImageData = container.OpenResource(res.StreamData);
            }
            return res;
        }


        public override Texture2D? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new Texture2D();
            UnityConverter.ReadTexture(res, reader, serializer);
            var version = reader.Get<Version>();

            res.Width = reader.ReadInt32();
            res.Height = reader.ReadInt32();
            var m_CompleteImageSize = reader.ReadInt32();
            if (version.GreaterThanOrEquals(2020)) //2020.1 and up
            {
                var m_MipsStripped = reader.ReadInt32();
            }
            res.TextureFormat = (TextureFormat)reader.ReadInt32();
            if (version.LessThan(5, 2)) //5.2 down
            {
                res.MipMap = reader.ReadBoolean();
            }
            else
            {
                res.MipCount = reader.ReadInt32();
            }
            if (version.GreaterThanOrEquals(2, 6)) //2.6.0 and up
            {
                var m_IsReadable = reader.ReadBoolean();
            
            }
            if (version.GreaterThanOrEquals(2020, 1)) //2020.1 and up
            {
                var m_IsPreProcessed = reader.ReadBoolean();
            }
            if (version.GreaterThanOrEquals(2019, 3)) //2019.3 and up
            {
                var m_IgnoreMasterTextureLimit = reader.ReadBoolean();
            }
            if (version.GreaterThanOrEquals(2022, 2)) //2022.2 and up
            {
                reader.AlignStream(); //m_IgnoreMipmapLimit
                var m_MipmapLimitGroupName = reader.ReadAlignedString();
            }
            if (version.GreaterThanOrEquals(3)) //3.0.0 - 5.4
            {
                if (version.LessThanOrEquals(5, 4))
                {
                    var m_ReadAllowed = reader.ReadBoolean();
                }
            }
            if (version.GreaterThanOrEquals(2018, 2)) //2018.2 and up
            {
                var m_StreamingMipmaps = reader.ReadBoolean();
            }
            reader.AlignStream();
            if (version.GreaterThanOrEquals(2018, 2)) //2018.2 and up
            {
                var m_StreamingMipmapsPriority = reader.ReadInt32();
            }
            var m_ImageCount = reader.ReadInt32();
            var m_TextureDimension = reader.ReadInt32();
            res.TextureSettings = serializer.Deserialize<GLTextureSettings>(reader);
            if (version.GreaterThanOrEquals(3)) //3.0 and up
            {
                var m_LightmapFormat = reader.ReadInt32();
            }
            if (version.GreaterThanOrEquals(3, 5)) //3.5.0 and up
            {
                var m_ColorSpace = reader.ReadInt32();
            }
            if (version.GreaterThanOrEquals(2020, 2)) //2020.2 and up
            {
                var m_PlatformBlob = reader.ReadArray(r => r.ReadByte());
                reader.AlignStream();
            }
            var imageDataSize = reader.ReadInt32();
            if (imageDataSize == 0 && version.GreaterThanOrEquals(5, 3))//5.3.0 and up
            {
                res.StreamData = serializer.Deserialize<ResourceSource>(reader);
            }

            if (!string.IsNullOrEmpty(res.StreamData.Source))
            {
                var container = reader.Get<ISerializedFile>();
                if (reader.TryGet<IDependencyBuilder>(out var builder))
                {
                    var fileName = container.FullPath;
                    var fileId = reader.Get<ObjectInfo>().FileID;
                    builder.AddDependencyEntry(fileName,
                        fileId,
                        res.StreamData.Source);
                }
                res.ImageData = container.OpenResource(res.StreamData);
            } else
            {
                res.ImageData = new PartialStream(reader.BaseStream, imageDataSize);
            }
            return res;
        }

        public void SaveAs(Texture2D res, string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".png", mode, out fileName))
            {
                return;
            }
            using var image = ToImage(res);
            using var target = image?.Flip(false);
            target?.SaveAs(fileName);
        }

        public static SKImage? ToImage(Texture2D res, Version version)
        {
            if (res.ImageData is null)
            {
                return null;
            }
            res.ImageData.Position = 0;
            var data = TextureExtension.Decode(res.ImageData.ToArray(), res.Width,
                res.Height, res.TextureFormat, version);
            return data?.ToImage();
        }
    }

    
}