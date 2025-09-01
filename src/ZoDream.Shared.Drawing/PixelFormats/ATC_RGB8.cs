using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace ZoDream.Shared.Drawing
{
    public class ATC_RGB8 : IBufferDecoder
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
                for (int s = 0; s < bcw; s++, inputOffset += 16)
                {
                    DecodeAtcRgba8Block(data.Slice(inputOffset, 16), buf);
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

        private static void DecodeAtcRgba8Block(ReadOnlySpan<byte> input, Span<byte> output)
        {
            Span<int> alphas = stackalloc int[16];
            ulong avalue = BinaryPrimitives.ReadUInt64LittleEndian(input);
            int a0 = unchecked((int)(avalue >> 0) & 0xFF);
            int a1 = unchecked((int)(avalue >> 8) & 0xFF);
            ulong aindex = avalue >> 16;

            Span<int> colors = stackalloc int[16];
            int c0 = BitConverter.ToUInt16(input[8..]);
            int c1 = BitConverter.ToUInt16(input[10..]);
            uint cindex = BitConverter.ToUInt32(input[12..]);

            ATC_RGB4.DecodeColors(colors, c0, c1);
            DecodeAlphas(alphas, a0, a1);

            for (int i = 0; i < 4 * 4; i++)
            {
                var colorOffset = unchecked((int)cindex & 3) * 4;
                var outputOffset = i * 4;
                output[outputOffset + 2] = (byte)colors[colorOffset + 0];
                output[outputOffset + 1] = (byte)colors[colorOffset + 1];
                output[outputOffset] = (byte)colors[colorOffset + 2];
                int aidx = unchecked((int)aindex & 7);
                output[outputOffset + 3] = (byte)alphas[aidx];
                cindex >>= 2;
                aindex >>= 3;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void DecodeAlphas(Span<int> alphas, int a0, int a1)
        {
            alphas[0] = a0;
            alphas[1] = a1;
            if (a0 > a1)
            {
                alphas[2] = (a0 * 6 + a1 * 1) / 7;
                alphas[3] = (a0 * 5 + a1 * 2) / 7;
                alphas[4] = (a0 * 4 + a1 * 3) / 7;
                alphas[5] = (a0 * 3 + a1 * 4) / 7;
                alphas[6] = (a0 * 2 + a1 * 5) / 7;
                alphas[7] = (a0 * 1 + a1 * 6) / 7;
            }
            else
            {
                alphas[2] = (a0 * 4 + a1 * 1) / 5;
                alphas[3] = (a0 * 3 + a1 * 2) / 5;
                alphas[4] = (a0 * 2 + a1 * 3) / 5;
                alphas[5] = (a0 * 1 + a1 * 4) / 5;
                alphas[6] = 0;
                alphas[7] = 255;
            }
        }
        
    }
}
