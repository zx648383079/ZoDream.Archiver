using System;
using System.Runtime.InteropServices;

namespace ZoDream.Shared.Drawing
{
    public class ETC2A8 : IBufferDecoder
    {
        public byte[] Decode(ReadOnlySpan<byte> data, int width, int height)
        {
            var buffer = new byte[width * height * 4];
            Decode(data, width, height, buffer);
            return buffer;
        }

        public int Decode(ReadOnlySpan<byte> data, int width, int height, Span<byte> output)
        {
            int requiredLength = ((width + 3) / 4) * ((height + 3) / 4) * 16;
            if (data.Length < requiredLength)
            {
                throw new ArgumentException(nameof(data));
            }

            int bcw = (width + 3) / 4;
            int bch = (height + 3) / 4;
            int clen_last = (width + 3) % 4 + 1;
            Span<byte> buf = stackalloc byte[16 * 4];
            int inputOffset = 0;
            for (int t = 0; t < bch; t++)
            {
                for (int s = 0; s < bcw; s++, inputOffset += 16)
                {
                    ETC2.DecodeEtc2Block(data.Slice(inputOffset + 8, 8), buf);
                    DecodeEtc2a8Block(data.Slice(inputOffset + 0, 8), buf);
                    int clen = s < bcw - 1 ? 4 : clen_last;
                    int outputOffset = t * 16 * width + s * 16;

                    for (int i = 0, y = t * 4; i < 4 && y < height; i++, y++)
                    {
                        for (int j = 0; j < clen; j++)
                        {
                            buf.Slice((j + 4 * i) * 4, 4).CopyTo(output[(outputOffset + (j + i * width) * 4)..]);
                        }
                    }
                }
            }
            return inputOffset;
        }

        private static void DecodeEtc2a8Block(ReadOnlySpan<byte> input, Span<byte> output)
        {
            int @base = input[0];
            int data1 = input[1];
            int mul = data1 >> 4;
            if (mul == 0)
            {
                for (int i = 0; i < 16; i++)
                {
                    EACRSigned.DecodeEac11Block(output[3..], @base);
                }
            }
            else
            {
                int table = data1 & 0xF;
                ulong l = ColorConverter.ReadUInt64From6Bit(input[2..]);
                EACRSigned.DecodeEac11Block(output[3..], @base, table, mul, l);
            }
        }
    }
}
