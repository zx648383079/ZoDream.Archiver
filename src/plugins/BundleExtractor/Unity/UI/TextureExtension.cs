using System;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.RustWrapper;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal static class TextureExtension
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
            Painter? painter = format switch
            {
                TextureFormat.ATC_RGB4 => new Painter(PixelID.AtcRgb, width, height, 4),
                TextureFormat.ATC_RGBA8 => new Painter(PixelID.AtcRgba, width, height, 8),
                TextureFormat.ASTC_HDR_4x4 => new Painter(PixelID.AsTcHdr, width, height, 4, 4),
                TextureFormat.ASTC_HDR_5x5 => new Painter(PixelID.AsTcHdr, width, height, 5, 5),
                TextureFormat.ASTC_HDR_6x6 => new Painter(PixelID.AsTcHdr, width, height, 6, 6),
                TextureFormat.ASTC_HDR_8x8 => new Painter(PixelID.AsTcHdr, width, height, 8, 8),
                TextureFormat.ASTC_HDR_10x10 => new Painter(PixelID.AsTcHdr, width, height, 10, 10),
                TextureFormat.ASTC_HDR_12x12 => new Painter(PixelID.AsTcHdr, width, height, 12, 12),
                TextureFormat.ASTC_RGB_4x4 => new Painter(PixelID.AsTcRgb, width, height, 4, 4),
                TextureFormat.ASTC_RGB_5x5 => new Painter(PixelID.AsTcRgb, width, height, 5, 5),
                TextureFormat.ASTC_RGB_6x6 => new Painter(PixelID.AsTcRgb, width, height, 6, 6),
                TextureFormat.ASTC_RGB_8x8 => new Painter(PixelID.AsTcRgb, width, height, 8, 8),
                TextureFormat.ASTC_RGB_10x10 => new Painter(PixelID.AsTcRgb, width, height, 10, 10),
                TextureFormat.ASTC_RGB_12x12 => new Painter(PixelID.AsTcRgb, width, height, 12, 12),
                TextureFormat.ASTC_RGBA_4x4 => new Painter(PixelID.AsTcRgba, width, height, 4, 4),
                TextureFormat.ASTC_RGBA_5x5 => new Painter(PixelID.AsTcRgba, width, height, 5, 5),
                TextureFormat.ASTC_RGBA_6x6 => new Painter(PixelID.AsTcRgba, width, height, 6, 6),
                TextureFormat.ASTC_RGBA_8x8 => new Painter(PixelID.AsTcRgba, width, height, 8, 8),
                TextureFormat.ASTC_RGBA_10x10 => new Painter(PixelID.AsTcRgba, width, height, 10, 10),
                TextureFormat.ASTC_RGBA_12x12 => new Painter(PixelID.AsTcRgba, width, height, 12, 12),
                TextureFormat.BC6H => new Painter(PixelID.Bcn, width, height, 6),
                TextureFormat.BC7 => new Painter(PixelID.Bcn, width, height, 7),
                TextureFormat.BC4 => new Painter(PixelID.Bcn, width, height, 4),
                TextureFormat.BC5 => new Painter(PixelID.Bcn, width, height, 5),
                TextureFormat.ETC2_RGB => new Painter(PixelID.EtcRgb, width, height, 2),
                TextureFormat.ETC_RGB4 or TextureFormat.ETC_RGB4_3DS
                    or TextureFormat.ETC_RGB4Crunched => new Painter(PixelID.EtcRgb, width, height, 1, 4),
                TextureFormat.EAC_R => new Painter(PixelID.EacR, width, height),
                TextureFormat.EAC_R_SIGNED => new Painter(PixelID.EacR, width, height, 1),
                TextureFormat.EAC_RG => new Painter(PixelID.EacRg, width, height),
                TextureFormat.EAC_RG_SIGNED => new Painter(PixelID.EacRg, width, height, 1),
                TextureFormat.ETC2_RGBA1 => new Painter(PixelID.EtcRgba, width, height, 2, 1),
                TextureFormat.ETC2_RGBA8 or TextureFormat.ETC2_RGBA8Crunched =>
                new Painter(PixelID.EtcRgba, width, height, 2, 8),
                TextureFormat.ETC_RGBA8_3DS => new Painter(PixelID.EtcRgba, width, height, 1, 8),
                TextureFormat.PVRTC_RGB2 => new Painter(PixelID.PvrTcRgb, width, height, 2),
                TextureFormat.PVRTC_RGBA2 => new Painter(PixelID.PvrTcRgba, width, height, 2),
                TextureFormat.PVRTC_RGB4 => new Painter(PixelID.PvrTcRgb, width, height, 4),
                TextureFormat.PVRTC_RGBA4 => new Painter(PixelID.PvrTcRgba, width, height, 4),
                _ => null
            };
            if (painter is null)
            {
                return BitmapFactory.Decode(data, width, height, Convert(format));
            }
            try
            {
                return BitmapFactory.Decode(painter.Decode(data), width, height, BitmapFormat.RGBA8888);
            }
            finally
            {
                painter?.Dispose();
            }
        }
    }
}
