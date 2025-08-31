using System;
using System.Runtime.CompilerServices;

namespace ZoDream.Shared.Drawing
{
    internal class EACRSigned : IBufferDecoder
    {
        public byte[] Decode(ReadOnlySpan<byte> data, int width, int height)
        {
            var buffer = new byte[width * height * 4];
            Decode(data, width, height, buffer);
            return buffer;
        }

        public int Decode(ReadOnlySpan<byte> data, int width, int height, Span<byte> output)
        {
            int requiredLength = ((width + 3) / 4) * ((height + 3) / 4) * 8;
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
                for (int s = 0; s < bcw; s++, inputOffset += 8)
                {
                    DecodeEacSignedBlock(data.Slice(inputOffset, 8), buf, 2);
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

        internal static void DecodeEacSignedBlock(ReadOnlySpan<byte> input, Span<byte> output, int channel)
        {
            int @base = 127 + unchecked((sbyte)input[0]);
            int data1 = input[1];
            int mul = data1 >> 4;
            if (mul == 0)
            {
                DecodeEac11Block(output[channel..], @base);
            }
            else
            {
                int table = data1 & 0xF;
                ulong l = ColorConverter.ReadUInt64From6Bit(input[2..]);
                DecodeEac11Block(output[channel..], @base, table, mul, l);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void DecodeEac11Block(Span<byte> output, int @base)
        {
            for (int i = 0; i < 16; i++)
            {
                output[ETC.WriteOrderTableRev[i] * 4] = (byte)(@base);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void DecodeEac11Block(Span<byte> output, int @base, int ti, int mul, ulong l)
        {
            ReadOnlySpan<sbyte> table = ETC.Etc2AlphaModTable.Slice(ti * 8, 8);
            for (int i = 0; i < 16; i++, l >>= 3)
            {
                int val = @base + mul * table[unchecked((int)(l & 0b111))];
                output[ETC.WriteOrderTableRev[i] * 4] = ColorConverter.Clamp(val);
            }
        }
    }
}
