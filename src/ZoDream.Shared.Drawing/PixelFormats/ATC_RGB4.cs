using System;
using System.Runtime.CompilerServices;

namespace ZoDream.Shared.Drawing
{
    /// <summary>
    /// @see https://github.com/AssetRipper/TextureDecoder
    /// </summary>
    public class ATC_RGB4 : BlockBufferDecoder
    {
        protected override void DecodeBlock(ReadOnlySpan<byte> data, Span<byte> output)
        {
            DecodeAtcRgb4Block(data, output);
        }



        private static void DecodeAtcRgb4Block(ReadOnlySpan<byte> input, Span<byte> output)
        {
            Span<int> colors = stackalloc int[16];
            int c0 = BitConverter.ToUInt16(input);
            int c1 = BitConverter.ToUInt16(input[2..]);
            uint cIndex = BitConverter.ToUInt32(input[4..]);

            DecodeColors(colors, c0, c1);

            for (var i = 0; i < 4 * 4; i++)
            {
                var colorOffset = unchecked((int)(cIndex & 3)) * 4;
                var outputOffset = i * 4;
                output[outputOffset + 2] = (byte)colors[colorOffset + 0];
                output[outputOffset + 1] = (byte)colors[colorOffset + 1];
                output[outputOffset] = (byte)colors[colorOffset + 2];
                output[outputOffset + 3] = byte.MaxValue;
                cIndex >>= 2;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void DecodeColors(Span<int> colors, int c0, int c1)
        {
            if ((c0 & 0x8000) == 0)
            {
                colors[0] = Extend((c0 >> 0) & 0x1F, 5, 8);
                colors[1] = Extend((c0 >> 5) & 0x1F, 5, 8);
                colors[2] = Extend((c0 >> 10) & 0x1F, 5, 8);

                colors[12] = Extend((c1 >> 0) & 0x1F, 5, 8);
                colors[13] = Extend((c1 >> 5) & 0x3f, 6, 8);
                colors[14] = Extend((c1 >> 11) & 0x1F, 5, 8);

                colors[4] = (5 * colors[0] + 3 * colors[12]) / 8;
                colors[5] = (5 * colors[1] + 3 * colors[13]) / 8;
                colors[6] = (5 * colors[2] + 3 * colors[14]) / 8;

                colors[8] = (3 * colors[0] + 5 * colors[12]) / 8;
                colors[9] = (3 * colors[1] + 5 * colors[13]) / 8;
                colors[10] = (3 * colors[2] + 5 * colors[14]) / 8;
            }
            else
            {
                colors[0] = 0;
                colors[1] = 0;
                colors[2] = 0;

                colors[8] = Extend((c0 >> 0) & 0x1F, 5, 8);
                colors[9] = Extend((c0 >> 5) & 0x1F, 5, 8);
                colors[10] = Extend((c0 >> 10) & 0x1F, 5, 8);

                colors[12] = Extend((c1 >> 0) & 0x1F, 5, 8);
                colors[13] = Extend((c1 >> 5) & 0x3F, 6, 8);
                colors[14] = Extend((c1 >> 11) & 0x1F, 5, 8);

                colors[4] = Math.Max(0, colors[8] - colors[12] / 4);
                colors[5] = Math.Max(0, colors[9] - colors[13] / 4);
                colors[6] = Math.Max(0, colors[10] - colors[14] / 4);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static int Extend(int value, int from, int to)
        {
            // bit-pattern replicating scaling (can at most double the bits)
            return (value << (to - from)) | (value >> (from * 2 - to));
        }


    }
}
