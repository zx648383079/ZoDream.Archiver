using System;
using UnityEngine;
using UnityEngine.Document;
using ZoDream.BundleExtractor.Unity.Document;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    

    internal class GLTextureSettingsConverter : BundleConverter<GLTextureSettings>
    {
        public override GLTextureSettings Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new GLTextureSettings();
            ReadBase(ref res, reader, serializer, () => { });
            return res;
        }

        public static void ReadBase(ref GLTextureSettings res, 
            IBundleBinaryReader reader, 
            IBundleSerializer serializer,
            Action cb)
        {
            var version = reader.Get<Version>();

            res.FilterMode = reader.ReadInt32();
            res.Aniso = reader.ReadInt32();
            res.MipBias = reader.ReadSingle();
            cb.Invoke();
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
        }
    }

    internal sealed class Texture2DConverter : BundleConverter<Texture2D>, ITypeTreeConverter
    {
        public object? Read(IBundleBinaryReader reader, Type targetType, VirtualDocument typeMaps)
        {
            var res = new Texture2D();
            var container = reader.Get<ISerializedFile>();
            new DocumentReader(container).Read(typeMaps, reader, res);
            if (!string.IsNullOrEmpty(res.StreamData.Source))
            {
                if (reader.TryGet<IDependencyBuilder>(out var builder))
                {
                    var sourcePath = container.FullPath;
                    var fileId = reader.Get<ObjectInfo>().FileID;
                    builder.AddDependencyEntry(sourcePath,
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
            res.TextureFormat = version.GreaterThanOrEquals(2023, 2, 0) ? 
                TextureExtension.Convert((TextureFormat_23)reader.ReadInt32()) 
                : (TextureFormat)reader.ReadInt32();
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
                    var sourcePath = container.FullPath;
                    var fileId = reader.Get<ObjectInfo>().FileID;
                    builder.AddDependencyEntry(sourcePath,
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


    }

    
}