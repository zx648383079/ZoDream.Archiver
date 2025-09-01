using System;
using System.Runtime.CompilerServices;

namespace ZoDream.Shared.Drawing
{
    public class ATC_RGB4 : IBufferDecoder
    {
        public byte[] Decode(ReadOnlySpan<byte> data, int width, int height)
        {
            var buffer = new byte[width * height * 4];
            Decode(data, width, height, buffer);
            return buffer;
        }

        public int Decode(ReadOnlySpan<byte> data, int width, int height, Span<byte> output)
        {
            int bcw = (width + 3) / 4;
            int bch = (height + 3) / 4;
            int clen_last = (width + 3) % 4 + 1;
            Span<byte> buf = stackalloc byte[16 * 4];
            int inputOffset = 0;
            for (int t = 0; t < bch; t++)
            {
                for (int s = 0; s < bcw; s++, inputOffset += 8)
                {
                    DecodeAtcRgb4Block(data.Slice(inputOffset, 8), buf);
                    int clen = s < bcw - 1 ? 4 : clen_last;
                    for (int i = 0, y = t * 4; i < 4 && y < height; i++, y++)
                    {
                        int outputOffset = t * 16 * width + s * 16 + i * width * 4;
                        for (int j = 0; j < clen; j++)
                        {
                            buf.Slice(j + 4 * i, 4).CopyTo(output[(outputOffset + j * 4)..]);
                        }
                    }
                }
            }
            return inputOffset;
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
