using SkiaSharp;
using System;
using System.Runtime.InteropServices;
using ZoDream.Shared.IO;

namespace ZoDream.Shared.Drawing.Pvr
{
    public static class PvtImage
    {
        public static readonly uint HeaderVersion = 0x03525650;
        private static readonly uint BadEndianess = 0x50565203;

        public static IImageData? Decode(byte[] buffer)
        {
            if (buffer[0] == 'P' && buffer[1] == 'V' && buffer[2] == 'R')
            {
                return DecodeV3(buffer);
            }
            return DecodeV2(buffer);
        }

        private static IImageData? DecodeV3(byte[] buffer)
        {
            var header = StructConvert.ToStruct<PVRv3TexHeader>(buffer);
            var pixelFormat = (PVR3TexturePixelFormat)header.PixelFormat;
            // PVR3TexturePixelFormat::BGRA8888, gfx::Format::BGRA8),
            // PVR3TexturePixelFormat::RGBA8888, gfx::Format::RGBA8),
            // PVR3TexturePixelFormat::RGBA4444, gfx::Format::RGBA4),
            // PVR3TexturePixelFormat::RGBA5551, gfx::Format::RGB5A1),
            // PVR3TexturePixelFormat::RGB565, gfx::Format::R5G6B5),
            // PVR3TexturePixelFormat::RGB888, gfx::Format::RGB8),
            // PVR3TexturePixelFormat::A8, gfx::Format::A8),
            // PVR3TexturePixelFormat::L8, gfx::Format::L8),
            // PVR3TexturePixelFormat::LA88, gfx::Format::LA8),

            // PVR3TexturePixelFormat::PVRTC2BPP_RGB, gfx::Format::PVRTC_RGB2),
            // PVR3TexturePixelFormat::PVRTC2BPP_RGBA, gfx::Format::PVRTC_RGBA2),
            // PVR3TexturePixelFormat::PVRTC4BPP_RGB, gfx::Format::PVRTC_RGB4),
            // PVR3TexturePixelFormat::PVRTC4BPP_RGBA, gfx::Format::PVRTC_RGBA4),

            // PVR3TexturePixelFormat::ETC1, gfx::Format::ETC_RGB8),
            var offset = Marshal.SizeOf<PVRv3TexHeader>() + (int)header.MetaDataLength;
            var data = Create(buffer, offset);
            return BitmapFactory.Decode(data, (int)header.Width,
                (int)header.Height, pixelFormat switch
                {
                    PVR3TexturePixelFormat.PVRTC2BPP_RGB => BitmapFormat.PVRTC_RGB2,
                    PVR3TexturePixelFormat.PVRTC2BPP_RGBA => BitmapFormat.PVRTC_RGBA2,
                    PVR3TexturePixelFormat.PVRTC4BPP_RGB => BitmapFormat.PVRTC_RGB4,
                    PVR3TexturePixelFormat.PVRTC4BPP_RGBA => BitmapFormat.PVRTC_RGBA4,
                    PVR3TexturePixelFormat.PVRTC2_2BPP_RGBA => BitmapFormat.PVRTC_RGBA2,
                    PVR3TexturePixelFormat.PVRTC2_4BPP_RGBA => BitmapFormat.PVRTC_RGBA4,
                    PVR3TexturePixelFormat.ETC1 => BitmapFormat.ETC_RGB8,
                    PVR3TexturePixelFormat.DXT1 => BitmapFormat.DXT1,
                    PVR3TexturePixelFormat.DXT2 => BitmapFormat.DXT2,
                    PVR3TexturePixelFormat.DXT3 => BitmapFormat.DXT3,
                    PVR3TexturePixelFormat.DXT4 => BitmapFormat.DXT4,
                    PVR3TexturePixelFormat.DXT5 => BitmapFormat.DXT5,
                    PVR3TexturePixelFormat.BC4 => throw new NotImplementedException(),
                    PVR3TexturePixelFormat.BC5 => throw new NotImplementedException(),
                    PVR3TexturePixelFormat.BC6 => throw new NotImplementedException(),
                    PVR3TexturePixelFormat.BC7 => throw new NotImplementedException(),
                    PVR3TexturePixelFormat.UYVY => throw new NotImplementedException(),
                    PVR3TexturePixelFormat.YUY2 => throw new NotImplementedException(),
                    PVR3TexturePixelFormat.B_W1BPP => throw new NotImplementedException(),
                    PVR3TexturePixelFormat.R9G9B9E5 => throw new NotImplementedException(),
                    PVR3TexturePixelFormat.RGBG8888 => throw new NotImplementedException(),
                    PVR3TexturePixelFormat.GRGB8888 => throw new NotImplementedException(),
                    PVR3TexturePixelFormat.ETC2_RGB => throw new NotImplementedException(),
                    PVR3TexturePixelFormat.ETC2_RGBA => throw new NotImplementedException(),
                    PVR3TexturePixelFormat.ETC2_RGBA1 => throw new NotImplementedException(),
                    PVR3TexturePixelFormat.EAC_R11_UNSIGNED => throw new NotImplementedException(),
                    PVR3TexturePixelFormat.EAC_R11_SIGNED => throw new NotImplementedException(),
                    PVR3TexturePixelFormat.EAC_R_G11_UNSIGNED => throw new NotImplementedException(),
                    PVR3TexturePixelFormat.EAC_R_G11_SIGNED => throw new NotImplementedException(),
                    PVR3TexturePixelFormat.BGRA8888 => BitmapFormat.BGRA8888,
                    PVR3TexturePixelFormat.RGBA8888 => BitmapFormat.RGBA8888,
                    PVR3TexturePixelFormat.RGBA4444 => BitmapFormat.RGBA4444,
                    PVR3TexturePixelFormat.RGBA5551 => BitmapFormat.RGBA5551,
                    PVR3TexturePixelFormat.RGB565 => BitmapFormat.RGB565,
                    PVR3TexturePixelFormat.RGB888 => BitmapFormat.RGB888,
                    PVR3TexturePixelFormat.A8 => BitmapFormat.A8,
                    PVR3TexturePixelFormat.L8 => BitmapFormat.L8,
                    PVR3TexturePixelFormat.LA88 => BitmapFormat.LA88,
                });
        }
        private static IImageData? DecodeV2(byte[] buffer)
        {
            var header = StructConvert.ToStruct<PVRv2TexHeader>(buffer);
            // if (header.PvrTag == "PVR!")
            var pixelFormat = (PVR2TexturePixelFormat)(header.Flags & 0xFF);
            var data = Create(buffer, Marshal.SizeOf<PVRv2TexHeader>());
            return BitmapFactory.Decode(data, (int)header.Width,
                (int)header.Height, pixelFormat switch
                {
                    PVR2TexturePixelFormat.RGBA4444 => BitmapFormat.RGBA4444,
                    PVR2TexturePixelFormat.RGBA5551 => BitmapFormat.RGBA5551,
                    PVR2TexturePixelFormat.RGBA8888 => BitmapFormat.RGBA8888,
                    PVR2TexturePixelFormat.RGB565 => BitmapFormat.RGB565,
                    PVR2TexturePixelFormat.RGB555 => BitmapFormat.RGB555,
                    PVR2TexturePixelFormat.RGB888 => BitmapFormat.RGB888,
                    PVR2TexturePixelFormat.I8 => BitmapFormat.L8,
                    PVR2TexturePixelFormat.AI88 => BitmapFormat.LA88,
                    PVR2TexturePixelFormat.PVRTC2BPP_RGBA => BitmapFormat.PVRTC_RGBA2,
                    PVR2TexturePixelFormat.PVRTC4BPP_RGBA => BitmapFormat.PVRTC_RGBA4,
                    PVR2TexturePixelFormat.BGRA8888 => BitmapFormat.BGRA8888,
                    PVR2TexturePixelFormat.A8 => BitmapFormat.A8,
                    _ => BitmapFormat.Unknown,
                });
        }

        private static byte[] Create(byte[] buffer, int offset)
        {
            var data = new byte[buffer.Length - offset];
            Buffer.BlockCopy(buffer, offset, data, 0, data.Length);
            return data;
        }

    }
}
