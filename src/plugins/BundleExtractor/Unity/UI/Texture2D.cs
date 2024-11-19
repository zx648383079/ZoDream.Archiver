using SkiaSharp;
using System;
using System.IO;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.UI
{
    

    public class GLTextureSettings
    {
        public int m_FilterMode;
        public int m_Aniso;
        public float m_MipBias;
        public int m_WrapMode;

        public GLTextureSettings() {}

        public GLTextureSettings(UIReader reader)
        {
            var version = reader.Version;

            m_FilterMode = reader.ReadInt32();
            m_Aniso = reader.ReadInt32();
            m_MipBias = reader.ReadSingle();
            if (reader.IsExAstris())
            {
                var m_TextureGroup = reader.ReadInt32();
            }
            if (version.Major >= 2017)//2017.x and up
            {
                m_WrapMode = reader.ReadInt32(); //m_WrapU
                int m_WrapV = reader.ReadInt32();
                int m_WrapW = reader.ReadInt32();
            }
            else
            {
                m_WrapMode = reader.ReadInt32();
            }
        }
    }

    public sealed class Texture2D : Texture, IFileWriter
    {
        public int m_Width;
        public int m_Height;
        public TextureFormat m_TextureFormat;
        public bool m_MipMap;
        public int m_MipCount;
        public GLTextureSettings m_TextureSettings;
        public Stream image_data;
        public StreamingInfo m_StreamData;

        private static bool HasGNFTexture(SerializedType type) =>
            Convert.ToHexString(type.OldTypeHash) == "1D52BB98AA5F54C67C22C39E8B2E400F";
        private static bool HasExternalMipRelativeOffset(SerializedType type)
        {
            return Convert.ToHexString(type.OldTypeHash) switch
            {
                "1D52BB98AA5F54C67C22C39E8B2E400F" or "5390A985F58D5524F95DB240E8789704" => true,
                _ => false
            };
        }

        public Texture2D(UIReader reader)
            : this (reader, true)
        {
            
        }

        public Texture2D(UIReader reader, bool isReadable) : 
            base(reader, isReadable)
        {
            if (!isReadable)
            {
                TypeTreeHelper.ReadType(reader.SerializedType.OldType, reader, this);
                if (!string.IsNullOrEmpty(m_StreamData?.path))
                {
                    image_data = reader.OpenResource(m_StreamData);
                }
                return;
            }
            var version = reader.Version;
            m_Width = reader.ReadInt32();
            m_Height = reader.ReadInt32();
            var m_CompleteImageSize = reader.ReadInt32();
            if (version.GreaterThanOrEquals(2020)) //2020.1 and up
            {
                var m_MipsStripped = reader.ReadInt32();
            }
            m_TextureFormat = (TextureFormat)reader.ReadInt32();
            if (version.LessThan(5, 2)) //5.2 down
            {
                m_MipMap = reader.ReadBoolean();
            }
            else
            {
                m_MipCount = reader.ReadInt32();
            }
            if (version.GreaterThanOrEquals(2, 6)) //2.6.0 and up
            {
                var m_IsReadable = reader.ReadBoolean();
                if (reader.IsGI() && HasGNFTexture(reader.SerializedType))
                {
                    var m_IsGNFTexture = reader.ReadBoolean();
                }
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
            if (reader.IsGI() && HasGNFTexture(reader.SerializedType))
            {
                var m_TextureGroup = reader.ReadInt32();
            }
            if (version.GreaterThanOrEquals(2018, 2)) //2018.2 and up
            {
                var m_StreamingMipmapsPriority = reader.ReadInt32();
            }
            var m_ImageCount = reader.ReadInt32();
            var m_TextureDimension = reader.ReadInt32();
            m_TextureSettings = new GLTextureSettings(reader);
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
            var image_data_size = reader.ReadInt32();
            if (image_data_size == 0 && version.GreaterThanOrEquals(5, 3))//5.3.0 and up
            {
                if (reader.IsGI() && HasExternalMipRelativeOffset(reader.SerializedType))
                {
                    var m_externalMipRelativeOffset = reader.ReadUInt32();
                }
                m_StreamData = new StreamingInfo(reader);
            }

            if (!string.IsNullOrEmpty(m_StreamData?.path))
            {
                image_data = reader.OpenResource(m_StreamData);
            } else
            {
                image_data = new PartialStream(reader.BaseStream, image_data_size);
            }
        }


        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".png", mode, out fileName))
            {
                return;
            }
            ToImage()?.SaveAs(fileName);
        }

        public SKImage? ToImage()
        {
            var data = TextureExtension.Decode(image_data.ToArray(), m_Width,
                m_Height, m_TextureFormat);
            return data?.ToImage();
        }
    }

    
}