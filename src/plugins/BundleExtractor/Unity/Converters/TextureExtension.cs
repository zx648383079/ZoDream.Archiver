using SkiaSharp;
using System;
using System.Buffers;
using System.IO;
using UnityEngine;
using ZoDream.BundleExtractor.Compression;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.IO;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal static class TextureExtension
    {
        public static TextureFormat Convert(TextureFormat_23 format)
        {
            if (Enum.TryParse<TextureFormat>(Enum.GetName(format), out var res))
            {
                return res;
            }
            throw new NotSupportedException(nameof(format));
        }

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
                TextureFormat.BC6H => BitmapFormat.BC6H,
                TextureFormat.BC7 => BitmapFormat.BC7,
                TextureFormat.BC4 => BitmapFormat.BC4,
                TextureFormat.BC5 => BitmapFormat.BC5,
             
                TextureFormat.PVRTC_RGB2 => BitmapFormat.PVRTC_RGB2,
                TextureFormat.PVRTC_RGBA2 => BitmapFormat.PVRTC_RGBA2,
                TextureFormat.PVRTC_RGB4 => BitmapFormat.PVRTC_RGB4,
                TextureFormat.PVRTC_RGBA4 => BitmapFormat.PVRTC_RGBA4,
                TextureFormat.ETC_RGB4 or TextureFormat.ETC_RGB4_3DS or TextureFormat.ETC_RGBA8_3DS => BitmapFormat.ETC,
                TextureFormat.ATC_RGB4 => BitmapFormat.ATC_RGB4,
                TextureFormat.ATC_RGBA8 => BitmapFormat.ATC_RGBA8,
                TextureFormat.EAC_R => BitmapFormat.EAC_R,
                TextureFormat.EAC_R_SIGNED => BitmapFormat.EAC_R_SIGNED,
                TextureFormat.EAC_RG => BitmapFormat.EAC_RG,
                TextureFormat.EAC_RG_SIGNED => BitmapFormat.EAC_RG_SIGNED,
                TextureFormat.ETC2_RGB => BitmapFormat.ETC2,
                TextureFormat.ETC2_RGBA1 => BitmapFormat.ETC2_A1,
                TextureFormat.ETC2_RGBA8 => BitmapFormat.ETC2_A8,

                TextureFormat.RG16 => BitmapFormat.RG88,
                TextureFormat.R8 => BitmapFormat.R8,
       
             
                TextureFormat.RG32 => BitmapFormat.RG1616,
                TextureFormat.RGB48 => BitmapFormat.RGB161616,
                TextureFormat.RGBA64 => BitmapFormat.RGBA16161616,

                // 一下需要特殊处理
                TextureFormat.ETC_RGB4Crunched => BitmapFormat.ETC,
                TextureFormat.ETC2_RGBA8Crunched => BitmapFormat.ETC2,
                TextureFormat.R16_Alt => BitmapFormat.R16,
                TextureFormat.DXT1Crunched => BitmapFormat.DXT1,
                TextureFormat.DXT5Crunched => BitmapFormat.DXT5,
                TextureFormat.ASTC_RGB_4x4
                or TextureFormat.ASTC_RGB_5x5
                or TextureFormat.ASTC_RGB_6x6
                or TextureFormat.ASTC_RGB_8x8
                or TextureFormat.ASTC_RGB_10x10
                or TextureFormat.ASTC_RGB_12x12
                or TextureFormat.ASTC_RGBA_4x4
                or TextureFormat.ASTC_RGBA_5x5
                or TextureFormat.ASTC_RGBA_6x6
                or TextureFormat.ASTC_RGBA_8x8
                or TextureFormat.ASTC_RGBA_10x10
                or TextureFormat.ASTC_RGBA_12x12
                or TextureFormat.ASTC_HDR_4x4
                or TextureFormat.ASTC_HDR_5x5
                or TextureFormat.ASTC_HDR_6x6
                or TextureFormat.ASTC_HDR_8x8
                or TextureFormat.ASTC_HDR_10x10
                or TextureFormat.ASTC_HDR_12x12 => BitmapFormat.ASTC,
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

        public static IImageData? Decode(Stream data, 
            int width, int height, 
            TextureFormat format, Version version)
        {
            if (data is null || data.Length == 0)
            {
                return null;
            }
            if (format.ToString().EndsWith("Crunched"))
            {
                if (version.GreaterThanOrEquals(2017, 3) || format is TextureFormat.ETC_RGB4Crunched or TextureFormat.ETC2_RGBA8Crunched)
                {
                    return BitmapFactory.Decode(new UnityCrunch(data).Read(), width, height, BitmapFormat.RGBA8888);
                }
                return BitmapFactory.Decode(new Crunch(data).Read(), width, height, BitmapFormat.RGBA8888);
            }
            IBufferDecoder? painter = format switch
            {
                TextureFormat.ASTC_HDR_4x4 => new ASTC(4, 4),
                TextureFormat.ASTC_HDR_5x5 => new ASTC(5, 5),
                TextureFormat.ASTC_HDR_6x6 => new ASTC(6, 6),
                TextureFormat.ASTC_HDR_8x8 => new ASTC(8, 8),
                TextureFormat.ASTC_HDR_10x10 => new ASTC(10, 10),
                TextureFormat.ASTC_HDR_12x12 => new ASTC(12, 12),
                TextureFormat.ASTC_RGB_4x4 => new ASTC(4, 4),
                TextureFormat.ASTC_RGB_5x5 => new ASTC(5, 5),
                TextureFormat.ASTC_RGB_6x6 => new ASTC(6, 6),
                TextureFormat.ASTC_RGB_8x8 => new ASTC(8, 8),
                TextureFormat.ASTC_RGB_10x10 => new ASTC(10, 10),
                TextureFormat.ASTC_RGB_12x12 => new ASTC(12, 12),
                TextureFormat.ASTC_RGBA_4x4 => new ASTC(4, 4),
                TextureFormat.ASTC_RGBA_5x5 => new ASTC(5, 5),
                TextureFormat.ASTC_RGBA_6x6 => new ASTC(6, 6),
                TextureFormat.ASTC_RGBA_8x8 => new ASTC(8, 8),
                TextureFormat.ASTC_RGBA_10x10 => new ASTC(10, 10),
                TextureFormat.ASTC_RGBA_12x12 => new ASTC(12, 12),
                _ => null
            };
            var targetFormat = Convert(format);
            if (painter is null && BitmapFactory.ShouldExtraDecoder(targetFormat))
            {
                painter = BitmapFactory.CreateDecoder(targetFormat);
                if (painter is null)
                {
                    return null;
                }
            }
            if (painter is null)
            {
                return BitmapFactory.Decode(data.ToArray(), width, height, targetFormat);
            }
            var length = (int)data.Length;
            var buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                data.Position = 0;
                data.ReadExactly(buffer, 0, length);
                return BitmapFactory.Decode(painter.Decode(buffer.AsSpan(0, length), width, height), width, height, SKColorType.Rgba8888);
            }
            catch (Exception e)
            {
                return null;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }


        public static SKImage? ClipAndFlip(this SKImage source, SKPath path, bool isHorizontal = true)
        {
            var rect = path.Bounds;
            if (rect.IsEmpty || rect.Width < 1 || rect.Height < 1)
            {
                return null;
            }
            var cx = rect.Width / 2;
            var cy = rect.Height / 2;
            return SkiaExtension.MutateImage((int)rect.Width, (int)rect.Height, canvas => {
                canvas.Translate(cx, cy);
                canvas.Flip(isHorizontal);
                canvas.Translate(-cx, -cy);
                canvas.DrawImage(source, rect,
                   SKRect.Create(0, 0, rect.Width, rect.Height));
                path.Offset(-rect.Left, -rect.Top);
                canvas.ClipPath(path, SKClipOperation.Difference);
                canvas.Clear();
            });
        }
    }
}
