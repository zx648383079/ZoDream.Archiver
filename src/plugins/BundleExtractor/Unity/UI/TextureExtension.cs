using System;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.RustWrapper;

namespace ZoDream.BundleExtractor.Unity.UI
{
    public static class TextureExtension
    {
        public static BitmapFormat Convert(TextureFormat textureFormat)
        {
            return textureFormat switch
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
                TextureFormat.RGBAFloat => BitmapFormat.RGBAF,
                TextureFormat.YUY2 => BitmapFormat.YUY2,
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
            };
        }

        public static TextureFormat Convert(GraphicsFormat graphicsFormat)
        {
            return graphicsFormat switch
            {
                GraphicsFormat.R8_SRGB or GraphicsFormat.R8_UInt or GraphicsFormat.R8_UNorm => TextureFormat.R8,
                GraphicsFormat.R8G8_SRGB or GraphicsFormat.R8G8_UInt or GraphicsFormat.R8G8_UNorm => TextureFormat.RG16,
                GraphicsFormat.R8G8B8_SRGB or GraphicsFormat.R8G8B8_UInt or GraphicsFormat.R8G8B8_UNorm => TextureFormat.RGB24,
                GraphicsFormat.R8G8B8A8_SRGB or GraphicsFormat.R8G8B8A8_UInt or GraphicsFormat.R8G8B8A8_UNorm => TextureFormat.RGBA32,
                GraphicsFormat.R16_UInt or GraphicsFormat.R16_UNorm => TextureFormat.R16,
                GraphicsFormat.R16G16_UInt or GraphicsFormat.R16G16_UNorm => TextureFormat.RG32,
                GraphicsFormat.R16G16B16_UInt or GraphicsFormat.R16G16B16_UNorm => TextureFormat.RGB48,
                GraphicsFormat.R16G16B16A16_UInt or GraphicsFormat.R16G16B16A16_UNorm => TextureFormat.RGBA64,
                GraphicsFormat.R16_SFloat => TextureFormat.RHalf,
                GraphicsFormat.R16G16_SFloat => TextureFormat.RGHalf,
                //?
                GraphicsFormat.R16G16B16_SFloat or GraphicsFormat.R16G16B16A16_SFloat => TextureFormat.RGBAHalf,
                GraphicsFormat.R32_SFloat => TextureFormat.RFloat,
                GraphicsFormat.R32G32_SFloat => TextureFormat.RGFloat,
                //?
                GraphicsFormat.R32G32B32_SFloat or GraphicsFormat.R32G32B32A32_SFloat => TextureFormat.RGBAFloat,
                GraphicsFormat.B8G8R8A8_SRGB or GraphicsFormat.B8G8R8A8_UInt or GraphicsFormat.B8G8R8A8_UNorm => TextureFormat.BGRA32,
                GraphicsFormat.E5B9G9R9_UFloatPack32 => TextureFormat.RGB9e5Float,
                GraphicsFormat.RGBA_DXT1_SRGB or GraphicsFormat.RGBA_DXT1_UNorm => TextureFormat.DXT1,
                GraphicsFormat.RGBA_DXT3_SRGB or GraphicsFormat.RGBA_DXT3_UNorm => TextureFormat.DXT3,
                GraphicsFormat.RGBA_DXT5_SRGB or GraphicsFormat.RGBA_DXT5_UNorm => TextureFormat.DXT5,
                GraphicsFormat.R_BC4_UNorm => TextureFormat.BC4,
                GraphicsFormat.RG_BC5_UNorm => TextureFormat.BC5,
                GraphicsFormat.RGB_BC6H_SFloat or GraphicsFormat.RGB_BC6H_UFloat => TextureFormat.BC6H,
                GraphicsFormat.RGBA_BC7_SRGB or GraphicsFormat.RGBA_BC7_UNorm => TextureFormat.BC7,
                GraphicsFormat.RGB_PVRTC_2Bpp_SRGB or GraphicsFormat.RGB_PVRTC_2Bpp_UNorm or GraphicsFormat.RGBA_PVRTC_2Bpp_SRGB or GraphicsFormat.RGBA_PVRTC_2Bpp_UNorm => TextureFormat.PVRTC_RGBA2,
                GraphicsFormat.RGB_PVRTC_4Bpp_SRGB or GraphicsFormat.RGB_PVRTC_4Bpp_UNorm or GraphicsFormat.RGBA_PVRTC_4Bpp_SRGB or GraphicsFormat.RGBA_PVRTC_4Bpp_UNorm => TextureFormat.PVRTC_RGBA4,
                GraphicsFormat.RGB_ETC_UNorm => TextureFormat.ETC_RGB4,
                GraphicsFormat.RGB_ETC2_SRGB or GraphicsFormat.RGB_ETC2_UNorm => TextureFormat.ETC2_RGB,
                GraphicsFormat.RGB_A1_ETC2_SRGB or GraphicsFormat.RGB_A1_ETC2_UNorm => TextureFormat.ETC2_RGBA1,
                GraphicsFormat.RGBA_ETC2_SRGB or GraphicsFormat.RGBA_ETC2_UNorm => TextureFormat.ETC2_RGBA8,
                GraphicsFormat.R_EAC_UNorm => TextureFormat.EAC_R,
                GraphicsFormat.R_EAC_SNorm => TextureFormat.EAC_R_SIGNED,
                GraphicsFormat.RG_EAC_UNorm => TextureFormat.EAC_RG,
                GraphicsFormat.RG_EAC_SNorm => TextureFormat.EAC_RG_SIGNED,
                GraphicsFormat.RGBA_ASTC4X4_SRGB or GraphicsFormat.RGBA_ASTC4X4_UNorm => TextureFormat.ASTC_RGBA_4x4,
                GraphicsFormat.RGBA_ASTC5X5_SRGB or GraphicsFormat.RGBA_ASTC5X5_UNorm => TextureFormat.ASTC_RGBA_5x5,
                GraphicsFormat.RGBA_ASTC6X6_SRGB or GraphicsFormat.RGBA_ASTC6X6_UNorm => TextureFormat.ASTC_RGBA_6x6,
                GraphicsFormat.RGBA_ASTC8X8_SRGB or GraphicsFormat.RGBA_ASTC8X8_UNorm => TextureFormat.ASTC_RGBA_8x8,
                GraphicsFormat.RGBA_ASTC10X10_SRGB or GraphicsFormat.RGBA_ASTC10X10_UNorm => TextureFormat.ASTC_RGBA_10x10,
                GraphicsFormat.RGBA_ASTC12X12_SRGB or GraphicsFormat.RGBA_ASTC12X12_UNorm => TextureFormat.ASTC_RGBA_12x12,
                GraphicsFormat.YUV2 or GraphicsFormat.VideoAuto => TextureFormat.YUY2,
                _ => 0,
            };
        }

        public static IImageData? Decode(byte[] data, 
            int width, int height, 
            TextureFormat format, UnityVersion version)
        {
            if (format.ToString().EndsWith("Crunched"))
            {
                using var decoder = new Painter(
                    version.GreaterThanOrEquals(2017, 3) ||
                    format == TextureFormat.ETC_RGB4Crunched ||
                    format == TextureFormat.ETC2_RGBA8Crunched ? PixelID.UnityCrunch : PixelID.Crunch
                    , width, height);
                data = decoder.Decode(data);
                format = Enum.Parse<TextureFormat>(format.ToString()[..^8]);
            }
            var id = format switch
            {
                TextureFormat.ATC_RGB4 => PixelID.Atc,
                TextureFormat.ATC_RGBA8 => PixelID.Atc,
                TextureFormat.ASTC_HDR_4x4 => PixelID.AsTc,
                TextureFormat.ASTC_HDR_5x5 => PixelID.AsTc,
                TextureFormat.ASTC_HDR_6x6 => PixelID.AsTc,
                TextureFormat.ASTC_HDR_8x8 => PixelID.AsTc,
                TextureFormat.ASTC_HDR_10x10 => PixelID.AsTc,
                TextureFormat.ASTC_HDR_12x12 => PixelID.AsTc,
                TextureFormat.ASTC_RGB_4x4 => PixelID.AsTc,
                TextureFormat.ASTC_RGB_5x5 => PixelID.AsTc,
                TextureFormat.ASTC_RGB_6x6 => PixelID.AsTc,
                TextureFormat.ASTC_RGB_8x8 => PixelID.AsTc,
                TextureFormat.ASTC_RGB_10x10 => PixelID.AsTc,
                TextureFormat.ASTC_RGB_12x12 => PixelID.AsTc,
                TextureFormat.ASTC_RGBA_4x4 => PixelID.AsTc,
                TextureFormat.ASTC_RGBA_5x5 => PixelID.AsTc,
                TextureFormat.ASTC_RGBA_6x6 => PixelID.AsTc,
                TextureFormat.ASTC_RGBA_8x8 => PixelID.AsTc,
                TextureFormat.ASTC_RGBA_10x10 => PixelID.AsTc,
                TextureFormat.ASTC_RGBA_12x12 => PixelID.AsTc,
                TextureFormat.BC6H => PixelID.Bcn,
                TextureFormat.BC7 => PixelID.Bcn,
                TextureFormat.BC4 => PixelID.Bcn,
                TextureFormat.BC5 => PixelID.Bcn,
                TextureFormat.ETC2_RGB => PixelID.Etc,
                TextureFormat.ETC_RGB4 or TextureFormat.ETC_RGB4_3DS 
                    or TextureFormat.ETC_RGB4Crunched => PixelID.Etc,
                TextureFormat.EAC_R => PixelID.Etc,
                TextureFormat.EAC_R_SIGNED => PixelID.Etc,
                TextureFormat.EAC_RG => PixelID.Etc,
                TextureFormat.EAC_RG_SIGNED => PixelID.Etc,
                TextureFormat.ETC2_RGBA1 => PixelID.Etc,
                TextureFormat.ETC2_RGBA8 or TextureFormat.ETC2_RGBA8Crunched => PixelID.Etc,
                TextureFormat.ETC_RGBA8_3DS => PixelID.Etc,
                TextureFormat.PVRTC_RGB2 => PixelID.PvrTc,
                TextureFormat.PVRTC_RGBA2 => PixelID.PvrTc,
                TextureFormat.PVRTC_RGB4 => PixelID.PvrTc,
                TextureFormat.PVRTC_RGBA4 => PixelID.PvrTc,
                _ => PixelID.Unknown
            };
            if (id == PixelID.Unknown)
            {
                return BitmapFactory.Decode(data, width, height, Convert(format));
            }
            using var painter = new Painter(id, width, height);
            return BitmapFactory.Decode(painter.Decode(data), width, height, BitmapFormat.RGBA8888);
        }
    }
}
