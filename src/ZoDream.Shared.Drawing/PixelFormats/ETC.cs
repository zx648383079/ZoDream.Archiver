using System;
using System.Runtime.CompilerServices;

namespace ZoDream.Shared.Drawing
{
    public class ETC : IBufferDecoder
    {
        public byte[] Decode(ReadOnlySpan<byte> data, int width, int height)
        {
            var buffer = new byte[width * height * 4];
            Decode(data, width, height, buffer);
            return buffer;
        }

        public int Decode(ReadOnlySpan<byte> data, int width, int height, Span<byte> output)
        {
            var requiredLength = (width + 3) / 4 * ((height + 3) / 4) * 8;
            if (requiredLength > data.Length)
            {
                throw new ArgumentException(null, nameof(data));
            }
            int bcw = (width + 3) / 4;
            int bch = (height + 3) / 4;
            int clen_last = (width + 3) % 4 + 1;
            Span<byte> buf = stackalloc byte[16 * 4];
            int inputOffset = 0;
            for (int t = 0; t < bch; t++)
            {
                for (int s = 0; s < bcw; s++, inputOffset += 8)
                {
                    DecodeEtc1Block(data.Slice(inputOffset, 8), buf);
                    int clen = s < bcw - 1 ? 4 : clen_last;
                    int outputOffset = t * 16 * width + s * 16;
                    var outputPtr = output.Slice(outputOffset);

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

        internal static void DecodeEtc1Block(ReadOnlySpan<byte> input, Span<byte> output)
        {
            byte i3 = input[3];
            ReadOnlySpan<int> code =
            [
                (i3 >> 5) * 4,
                (i3 >> 2 & 7) * 4
            ];
            Span<byte> c = stackalloc byte[6];
            int ti = (i3 & 1) * 16;
            if ((i3 & 2) != 0)
            {
                unchecked
                {
                    c[0] = (byte)(input[0] & 0xf8);
                    c[1] = (byte)(input[1] & 0xf8);
                    c[2] = (byte)(input[2] & 0xf8);
                    c[3] = (byte)(c[0] + (input[0] << 3 & 0x18) - (input[0] << 3 & 0x20));
                    c[4] = (byte)(c[1] + (input[1] << 3 & 0x18) - (input[1] << 3 & 0x20));
                    c[5] = (byte)(c[2] + (input[2] << 3 & 0x18) - (input[2] << 3 & 0x20));
                    c[0] |= (byte)(c[0] >> 5);
                    c[1] |= (byte)(c[1] >> 5);
                    c[2] |= (byte)(c[2] >> 5);
                    c[3] |= (byte)(c[3] >> 5);
                    c[4] |= (byte)(c[4] >> 5);
                    c[5] |= (byte)(c[5] >> 5);
                }
            }
            else
            {
                unchecked
                {
                    c[0] = (byte)(input[0] & 0xf0 | input[0] >> 4);
                    c[3] = (byte)(input[0] & 0x0f | input[0] << 4);
                    c[1] = (byte)(input[1] & 0xf0 | input[1] >> 4);
                    c[4] = (byte)(input[1] & 0x0f | input[1] << 4);
                    c[2] = (byte)(input[2] & 0xf0 | input[2] >> 4);
                    c[5] = (byte)(input[2] & 0x0f | input[2] << 4);
                }
            }

            int j = input[6] << 8 | input[7];
            int k = input[4] << 8 | input[5];
            for (int i = 0; i < 16; i++, j >>= 1, k >>= 1)
            {
                int s = Etc1SubblockTable[ti + i];
                int index = k << 1 & 2 | j & 1;
                int cd = code[s];
                int m = Etc1ModifierTable[cd + index];
                ApplicateColor(c, s, m, output[(WriteOrderTable[i] * 4)..]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void ApplicateColor(ReadOnlySpan<byte> c, int o, int m, Span<byte> output)
        {
            output[0] = ColorConverter.Clamp(c[o * 3 + 0] + m);
            output[1] = ColorConverter.Clamp(c[o * 3 + 1] + m);
            output[2] = ColorConverter.Clamp(c[o * 3 + 2] + m);
            output[3] = byte.MaxValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal static void ApplicateColorRaw(ReadOnlySpan<byte> c, int o, Span<byte> output)
        {
            c.Slice(o * 3, 3).CopyTo(output);
            output[3] = byte.MaxValue;
        }


        internal static ReadOnlySpan<byte> WriteOrderTable => [0, 4, 8, 12, 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15];
        internal static ReadOnlySpan<byte> WriteOrderTableRev => [15, 11, 7, 3, 14, 10, 6, 2, 13, 9, 5, 1, 12, 8, 4, 0];
        internal static ReadOnlySpan<int> Etc1SubblockTable =>
        [
            0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1,
            0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1,
        ];
        internal static ReadOnlySpan<int> Etc1ModifierTable =>
        [
            2, 8, -2, -8,
            5, 17, -5, -17,
            9, 29, -9, -29,
            13, 42, -13, -42,
            18, 60, -18, -60,
            24, 80, -24, -80,
            33, 106, -33, -106,
            47, 183, -47, -183,
        ];
        internal static ReadOnlySpan<int> PunchthroughModifierTable =>
        [
            0, 8, 0, -8,
            0, 17, 0, -17,
            0, 29, 0, -29,
            0, 42, 0, -42,
            0, 60, 0, -60,
            0, 80, 0, -80,
            0, 106, 0, -106,
            0, 183, 0, -183,
        ];
        internal static ReadOnlySpan<byte> Etc2DistanceTable => [3, 6, 11, 16, 23, 32, 41, 64];
        internal static ReadOnlySpan<sbyte> Etc2AlphaModTable =>
        [
            -3, -6,  -9, -15, 2, 5, 8, 14,
            -3, -7, -10, -13, 2, 6, 9, 12,
            -2, -5,  -8, -13, 1, 4, 7, 12,
            -2, -4,  -6, -13, 1, 3, 5, 12,
            -3, -6,  -8, -12, 2, 5, 7, 11,
            -3, -7,  -9, -11, 2, 6, 8, 10,
            -4, -7,  -8, -11, 3, 6, 7, 10,
            -3, -5,  -8, -11, 2, 4, 7, 10,
            -2, -6,  -8, -10, 1, 5, 7,  9,
            -2, -5,  -8, -10, 1, 4, 7,  9,
            -2, -4,  -8, -10, 1, 3, 7,  9,
            -2, -5,  -7, -10, 1, 4, 6,  9,
            -3, -4,  -7, -10, 2, 3, 6,  9,
            -1, -2,  -3, -10, 0, 1, 2,  9,
            -4, -6,  -8,  -9, 3, 5, 7,  8,
            -3, -5,  -7,  -9, 2, 4, 6,  8,
        ];
        internal static ReadOnlySpan<uint> PunchthroughMaskTable => [0xFFFFFFFF, 0xFFFFFFFF, 0x00000000, 0xFFFFFFFF];
    }
}
