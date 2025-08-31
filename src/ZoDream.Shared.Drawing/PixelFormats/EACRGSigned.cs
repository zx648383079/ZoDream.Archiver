using System;
using System.Runtime.InteropServices;

namespace ZoDream.Shared.Drawing
{
    internal class EACRGSigned : IBufferDecoder
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
            for (int i = 0; i < 16; i++)
            {
                buf[i * 4 + 3] = byte.MaxValue;
            }
            int inputOffset = 0;
            for (int t = 0; t < bch; t++)
            {
                for (int s = 0; s < bcw; s++, inputOffset += 16)
                {
                    EACRSigned.DecodeEacSignedBlock(data.Slice(inputOffset + 0, 8), buf, 2);
                    EACRSigned.DecodeEacSignedBlock(data.Slice(inputOffset + 8, 8), buf, 1);
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
    }
}
