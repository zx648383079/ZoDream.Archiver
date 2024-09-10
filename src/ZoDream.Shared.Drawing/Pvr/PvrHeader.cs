using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Shared.Drawing.Pvr
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PVRv3TexHeader
    {
        public uint Version;
        public uint Flags;
        public ulong PixelFormat;
        public uint ColorSpace;
        public uint ChannelType;
        public uint Height;
        public uint Width;
        public uint Depth;
        public uint NumberOfSurfaces;
        public uint NumberOfFaces;
        public uint NumberOfMipmaps;
        public uint MetaDataLength;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PVRv2TexHeader
    {
        public uint HeaderLength;
        public uint Height;
        public uint Width;
        public uint NumMipmaps;
        public uint Flags;
        public uint DataLength;
        public uint Bpp;
        public uint BitmaskRed;
        public uint BitmaskGreen;
        public uint BitmaskBlue;
        public uint BitmaskAlpha;
        public uint PvrTag;
        public uint NumSurfs;
    }
}
