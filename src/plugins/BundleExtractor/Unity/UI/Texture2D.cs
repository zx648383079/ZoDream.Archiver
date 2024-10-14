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
    public class StreamingInfo
    {
        public long offset; //ulong
        public uint size;
        public string path;

        public StreamingInfo(UIReader reader)
        {
            var version = reader.Version;

            if (version.Major >= 2020) //2020.1 and up
            {
                offset = reader.Reader.ReadInt64();
            }
            else
            {
                offset = reader.Reader.ReadUInt32();
            }
            size = reader.Reader.ReadUInt32();
            path = reader.ReadAlignedString();
        }
    }

    public class GLTextureSettings
    {
        public int m_FilterMode;
        public int m_Aniso;
        public float m_MipBias;
        public int m_WrapMode;

        public GLTextureSettings(UIReader reader)
        {
            var version = reader.Version;

            m_FilterMode = reader.Reader.ReadInt32();
            m_Aniso = reader.Reader.ReadInt32();
            m_MipBias = reader.Reader.ReadSingle();
            if (reader.IsExAstris())
            {
                var m_TextureGroup = reader.Reader.ReadInt32();
            }
            if (version.Major >= 2017)//2017.x and up
            {
                m_WrapMode = reader.Reader.ReadInt32(); //m_WrapU
                int m_WrapV = reader.Reader.ReadInt32();
                int m_WrapW = reader.Reader.ReadInt32();
            }
            else
            {
                m_WrapMode = reader.Reader.ReadInt32();
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

        public Texture2D(UIReader reader) : base(reader)
        {
            var version = reader.Version;
            m_Width = reader.Reader.ReadInt32();
            m_Height = reader.Reader.ReadInt32();
            var m_CompleteImageSize = reader.Reader.ReadInt32();
            if (version.GreaterThanOrEquals(2020)) //2020.1 and up
            {
                var m_MipsStripped = reader.Reader.ReadInt32();
            }
            m_TextureFormat = (TextureFormat)reader.Reader.ReadInt32();
            if (version.LessThan(5, 2)) //5.2 down
            {
                m_MipMap = reader.Reader.ReadBoolean();
            }
            else
            {
                m_MipCount = reader.Reader.ReadInt32();
            }
            if (version.GreaterThanOrEquals(2, 6)) //2.6.0 and up
            {
                var m_IsReadable = reader.Reader.ReadBoolean();
                if (reader.IsGI() && HasGNFTexture(reader.SerializedType))
                {
                    var m_IsGNFTexture = reader.Reader.ReadBoolean();
                }
            }
            if (version.GreaterThanOrEquals(2020, 1)) //2020.1 and up
            {
                var m_IsPreProcessed = reader.Reader.ReadBoolean();
            }
            if (version.GreaterThanOrEquals(2019, 3)) //2019.3 and up
            {
                var m_IgnoreMasterTextureLimit = reader.Reader.ReadBoolean();
            }
            if (version.GreaterThanOrEquals(2022, 2)) //2022.2 and up
            {
                reader.Reader.AlignStream(); //m_IgnoreMipmapLimit
                var m_MipmapLimitGroupName = reader.ReadAlignedString();
            }
            if (version.GreaterThanOrEquals(3)) //3.0.0 - 5.4
            {
                if (version.LessThanOrEquals(5, 4))
                {
                    var m_ReadAllowed = reader.Reader.ReadBoolean();
                }
            }
            if (version.GreaterThanOrEquals(2018, 2)) //2018.2 and up
            {
                var m_StreamingMipmaps = reader.Reader.ReadBoolean();
            }
            reader.Reader.AlignStream();
            if (reader.IsGI() && HasGNFTexture(reader.SerializedType))
            {
                var m_TextureGroup = reader.Reader.ReadInt32();
            }
            if (version.GreaterThanOrEquals(2018, 2)) //2018.2 and up
            {
                var m_StreamingMipmapsPriority = reader.Reader.ReadInt32();
            }
            var m_ImageCount = reader.Reader.ReadInt32();
            var m_TextureDimension = reader.Reader.ReadInt32();
            m_TextureSettings = new GLTextureSettings(reader);
            if (version.GreaterThanOrEquals(3)) //3.0 and up
            {
                var m_LightmapFormat = reader.Reader.ReadInt32();
            }
            if (version.GreaterThanOrEquals(3, 5)) //3.5.0 and up
            {
                var m_ColorSpace = reader.Reader.ReadInt32();
            }
            if (version.GreaterThanOrEquals(2020, 2)) //2020.2 and up
            {
                var m_PlatformBlob = reader.Reader.ReadArray(r => r.ReadByte());
                reader.Reader.AlignStream();
            }
            var image_data_size = reader.Reader.ReadInt32();
            if (image_data_size == 0 && version.GreaterThanOrEquals(5, 3))//5.3.0 and up
            {
                if (reader.IsGI() && HasExternalMipRelativeOffset(reader.SerializedType))
                {
                    var m_externalMipRelativeOffset = reader.Reader.ReadUInt32();
                }
                m_StreamData = new StreamingInfo(reader);
            }

            //ResourceReader resourceReader;
            //if (!string.IsNullOrEmpty(m_StreamData?.path))
            //{
            //    resourceReader = new ResourceReader(m_StreamData.path, assetsFile, m_StreamData.offset, m_StreamData.size);
            //}
            //else
            //{
            //    resourceReader = new ResourceReader(reader, reader.BaseStream.Position, image_data_size);
            //}
            image_data = new PartialStream(reader.Reader.BaseStream, image_data_size);
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
            var data = BitmapFactory.Decode(image_data.ToArray(), m_Width,
                m_Height, m_TextureFormat switch
                {
                    TextureFormat.Alpha8 => BitmapFormat.A8,
                    TextureFormat.ARGB4444 => BitmapFormat.RGBA4444,
                    TextureFormat.RGB24 => BitmapFormat.RGB888,
                    TextureFormat.RGBA32 => BitmapFormat.RGBA8888,
                    TextureFormat.ARGB32 => BitmapFormat.ARGB8888,
                    TextureFormat.ARGBFloat => BitmapFormat.ARGBF,
                    TextureFormat.RGB565 => BitmapFormat.RGB565,
                    TextureFormat.BGR24 => BitmapFormat.BGR888,
                    TextureFormat.R16 => BitmapFormat.R16,
                    TextureFormat.DXT1 => BitmapFormat.DXT1,
                    TextureFormat.DXT3 => BitmapFormat.DXT3,
                    TextureFormat.DXT5 => BitmapFormat.DXT5,
                    TextureFormat.RGBA4444 => BitmapFormat.RGBA4444,
                    TextureFormat.BGRA32 => BitmapFormat.BGRA8888,
                    TextureFormat.RHalf => BitmapFormat.RH,
                    TextureFormat.RGHalf => BitmapFormat.RGH,
                    TextureFormat.RGBAHalf => BitmapFormat.RGBAH,
                    TextureFormat.RFloat => BitmapFormat.RF,
                    TextureFormat.RGFloat => BitmapFormat.RGF,
                    TextureFormat.RGBAFloat => throw new NotImplementedException(),
                    TextureFormat.YUY2 => throw new NotImplementedException(),
                    TextureFormat.RGB9e5Float => BitmapFormat.RGB9e5F,
                    TextureFormat.RGBFloat => BitmapFormat.RGBF,
                    TextureFormat.BC6H => throw new NotImplementedException(),
                    TextureFormat.BC7 => throw new NotImplementedException(),
                    TextureFormat.BC4 => throw new NotImplementedException(),
                    TextureFormat.BC5 => throw new NotImplementedException(),
                    TextureFormat.DXT1Crunched => throw new NotImplementedException(),
                    TextureFormat.DXT5Crunched => throw new NotImplementedException(),
                    TextureFormat.PVRTC_RGB2 => BitmapFormat.PVRTC_RGB2,
                    TextureFormat.PVRTC_RGBA2 => BitmapFormat.PVRTC_RGBA2,
                    TextureFormat.PVRTC_RGB4 => BitmapFormat.PVRTC_RGB4,
                    TextureFormat.PVRTC_RGBA4 => BitmapFormat.PVRTC_RGBA4,
                    TextureFormat.ETC_RGB4 => throw new NotImplementedException(),
                    TextureFormat.ATC_RGB4 => throw new NotImplementedException(),
                    TextureFormat.ATC_RGBA8 => throw new NotImplementedException(),
                    TextureFormat.EAC_R => throw new NotImplementedException(),
                    TextureFormat.EAC_R_SIGNED => throw new NotImplementedException(),
                    TextureFormat.EAC_RG => throw new NotImplementedException(),
                    TextureFormat.EAC_RG_SIGNED => throw new NotImplementedException(),
                    TextureFormat.ETC2_RGB => throw new NotImplementedException(),
                    TextureFormat.ETC2_RGBA1 => throw new NotImplementedException(),
                    TextureFormat.ETC2_RGBA8 => throw new NotImplementedException(),
                    TextureFormat.ASTC_RGB_4x4 => throw new NotImplementedException(),
                    TextureFormat.ASTC_RGB_5x5 => throw new NotImplementedException(),
                    TextureFormat.ASTC_RGB_6x6 => throw new NotImplementedException(),
                    TextureFormat.ASTC_RGB_8x8 => throw new NotImplementedException(),
                    TextureFormat.ASTC_RGB_10x10 => throw new NotImplementedException(),
                    TextureFormat.ASTC_RGB_12x12 => throw new NotImplementedException(),
                    TextureFormat.ASTC_RGBA_4x4 => throw new NotImplementedException(),
                    TextureFormat.ASTC_RGBA_5x5 => throw new NotImplementedException(),
                    TextureFormat.ASTC_RGBA_6x6 => throw new NotImplementedException(),
                    TextureFormat.ASTC_RGBA_8x8 => throw new NotImplementedException(),
                    TextureFormat.ASTC_RGBA_10x10 => throw new NotImplementedException(),
                    TextureFormat.ASTC_RGBA_12x12 => throw new NotImplementedException(),
                    TextureFormat.ETC_RGB4_3DS => throw new NotImplementedException(),
                    TextureFormat.ETC_RGBA8_3DS => throw new NotImplementedException(),
                    TextureFormat.RG16 => BitmapFormat.RG88,
                    TextureFormat.R8 => BitmapFormat.R8,
                    TextureFormat.ETC_RGB4Crunched => throw new NotImplementedException(),
                    TextureFormat.ETC2_RGBA8Crunched => throw new NotImplementedException(),
                    TextureFormat.R16_Alt => throw new NotImplementedException(),
                    TextureFormat.ASTC_HDR_4x4 => throw new NotImplementedException(),
                    TextureFormat.ASTC_HDR_5x5 => throw new NotImplementedException(),
                    TextureFormat.ASTC_HDR_6x6 => throw new NotImplementedException(),
                    TextureFormat.ASTC_HDR_8x8 => throw new NotImplementedException(),
                    TextureFormat.ASTC_HDR_10x10 => throw new NotImplementedException(),
                    TextureFormat.ASTC_HDR_12x12 => throw new NotImplementedException(),
                    TextureFormat.RG32 => BitmapFormat.RG1616,
                    TextureFormat.RGB48 => BitmapFormat.RGB161616,
                    TextureFormat.RGBA64 => BitmapFormat.RGBA16161616,
                    _ => BitmapFormat.BGRA8888,
                });

            return data.ToImage();
        }
    }

    public enum TextureFormat
    {
        Alpha8 = 1,
        ARGB4444,
        RGB24,
        RGBA32,
        ARGB32,
        ARGBFloat,
        RGB565,
        BGR24,
        R16,
        DXT1,
        DXT3,
        DXT5,
        RGBA4444,
        BGRA32,
        RHalf,
        RGHalf,
        RGBAHalf,
        RFloat,
        RGFloat,
        RGBAFloat,
        YUY2,
        RGB9e5Float,
        RGBFloat,
        BC6H,
        BC7,
        BC4,
        BC5,
        DXT1Crunched,
        DXT5Crunched,
        PVRTC_RGB2,
        PVRTC_RGBA2,
        PVRTC_RGB4,
        PVRTC_RGBA4,
        ETC_RGB4,
        ATC_RGB4,
        ATC_RGBA8,
        EAC_R = 41,
        EAC_R_SIGNED,
        EAC_RG,
        EAC_RG_SIGNED,
        ETC2_RGB,
        ETC2_RGBA1,
        ETC2_RGBA8,
        ASTC_RGB_4x4,
        ASTC_RGB_5x5,
        ASTC_RGB_6x6,
        ASTC_RGB_8x8,
        ASTC_RGB_10x10,
        ASTC_RGB_12x12,
        ASTC_RGBA_4x4,
        ASTC_RGBA_5x5,
        ASTC_RGBA_6x6,
        ASTC_RGBA_8x8,
        ASTC_RGBA_10x10,
        ASTC_RGBA_12x12,
        ETC_RGB4_3DS,
        ETC_RGBA8_3DS,
        RG16,
        R8,
        ETC_RGB4Crunched,
        ETC2_RGBA8Crunched,
        R16_Alt,
        ASTC_HDR_4x4,
        ASTC_HDR_5x5,
        ASTC_HDR_6x6,
        ASTC_HDR_8x8,
        ASTC_HDR_10x10,
        ASTC_HDR_12x12,
        RG32,
        RGB48,
        RGBA64
    }
}