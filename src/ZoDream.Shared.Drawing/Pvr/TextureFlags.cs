using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Shared.Drawing.Pvr
{
    public enum PvrTextureFlags
    {
        No_Flag = 0,
        Pre_Multiplied = 0x2
    }

    public enum PVR2TexturePixelFormat
    {
        RGBA4444 = 0x10,
        RGBA5551,
        RGBA8888,
        RGB565,
        RGB555,
        RGB888,
        I8,
        AI88,
        PVRTC2BPP_RGBA,
        PVRTC4BPP_RGBA,
        BGRA8888,
        A8,
    }
    public enum PVR3TexturePixelFormat : ulong
    {
        PVRTC2BPP_RGB = 0UL,
        PVRTC2BPP_RGBA = 1UL,
        PVRTC4BPP_RGB = 2UL,
        PVRTC4BPP_RGBA = 3UL,
        PVRTC2_2BPP_RGBA = 4UL,
        PVRTC2_4BPP_RGBA = 5UL,
        ETC1 = 6UL,
        DXT1 = 7UL,
        DXT2 = 8UL,
        DXT3 = 9UL,
        DXT4 = 10UL,
        DXT5 = 11UL,
        BC1 = 7UL,
        BC2 = 9UL,
        BC3 = 11UL,
        BC4 = 12UL,
        BC5 = 13UL,
        BC6 = 14UL,
        BC7 = 15UL,
        UYVY = 16UL,
        YUY2 = 17UL,
        B_W1BPP = 18UL,
        R9G9B9E5 = 19UL,
        RGBG8888 = 20UL,
        GRGB8888 = 21UL,
        ETC2_RGB = 22UL,
        ETC2_RGBA = 23UL,
        ETC2_RGBA1 = 24UL,
        EAC_R11_UNSIGNED = 25UL,
        EAC_R11_SIGNED = 26UL,
        EAC_R_G11_UNSIGNED = 27UL,
        EAC_R_G11_SIGNED = 28UL,

        BGRA8888 = 0x0808080861726762UL,
        RGBA8888 = 0x0808080861626772UL,
        RGBA4444 = 0x0404040461626772UL,
        RGBA5551 = 0x0105050561626772UL,
        RGB565 = 0x0005060500626772UL,
        RGB888 = 0x0008080800626772UL,
        A8 = 0x0000000800000061UL,
        L8 = 0x000000080000006cUL,
        LA88 = 0x000008080000616cUL,
    }

    public enum PvrChannelType
    {
        UByte_Norm = 0,
        SByte_Norm = 1,
        UByte = 2,
        SByte = 3,
        UShort_Norm = 4,
        Short_Norm = 5,
        UShort = 6,
        Short = 7,
        UInt_Norm = 8,
        Int_Norm = 9,
        UInt = 10,
        Int = 11,
        Float = 12,
    }
}
