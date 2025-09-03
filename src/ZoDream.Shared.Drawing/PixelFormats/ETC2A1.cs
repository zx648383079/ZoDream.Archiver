using System;

namespace ZoDream.Shared.Drawing
{
    /// <summary>
    /// @see https://github.com/AssetRipper/TextureDecoder
    /// </summary>
    public class ETC2A1 : BlockBufferDecoder
    {
        protected override void DecodeBlock(ReadOnlySpan<byte> data, Span<byte> output)
        {
            if ((data[3] & 2) != 0)
            {
                // Opaque
                ETC2.DecodeEtc2Block(data, output);
            }
            else
            {
                DecodeEtc2PunchThrowBlock(data, output);
            }
        }

        private static void DecodeEtc2PunchThrowBlock(ReadOnlySpan<byte> input, Span<byte> output)
        {
            int j = input[6] << 8 | input[7];
            int k = input[4] << 8 | input[5];
            Span<byte> c = stackalloc byte[3 * 3];
            Span<byte> color_set = stackalloc byte[4 * 4];
            int r = input[0] & 0xf8;
            int dr = (input[0] << 3 & 0x18) - (input[0] << 3 & 0x20);
            if (r + dr < 0 || r + dr > 255)
            {
                // T (Etc2Block + mask for color)
                unchecked
                {
                    c[0] = (byte)(input[0] << 3 & 0xc0 | input[0] << 4 & 0x30 | input[0] >> 1 & 0xc | input[0] & 3);
                    c[1] = (byte)(input[1] & 0xf0 | input[1] >> 4);
                    c[2] = (byte)(input[1] & 0x0f | input[1] << 4);
                    c[3] = (byte)(input[2] & 0xf0 | input[2] >> 4);
                    c[4] = (byte)(input[2] & 0x0f | input[2] << 4);
                    c[5] = (byte)(input[3] & 0xf0 | input[3] >> 4);
                }
                int ti = input[3] >> 1 & 6 | input[3] & 1;
                byte d = ETC.Etc2DistanceTable[ti];
                ETC.ApplicateColorRaw(c, 0, color_set);
                ETC.ApplicateColor(c, 1, d, color_set[4..]);
                ETC.ApplicateColorRaw(c, 1, color_set[8..]);
                ETC.ApplicateColor(c, 1, -d, color_set[12..]);

                for (int i = 0; i < 16; i++, j >>= 1, k >>= 1)
                {
                    int index = k << 1 & 2 | j & 1;
                    uint color = color_set[index];
                    WriteMask(color_set[(index * 4)..], ETC.PunchthroughMaskTable[index], output[(ETC.WriteOrderTable[i] * 4)..]);
                }
            }
            else
            {
                int g = input[1] & 0xf8;
                int dg = (input[1] << 3 & 0x18) - (input[1] << 3 & 0x20);
                if (g + dg < 0 || g + dg > 255)
                {
                    // H (Etc2Block + mask for color)
                    unchecked
                    {
                        c[0] = (byte)(input[0] << 1 & 0xf0 | input[0] >> 3 & 0xf);
                        c[1] = (byte)(input[0] << 5 & 0xe0 | input[1] & 0x10);
                        c[1] |= (byte)(c[0 * 3 + 1] >> 4);
                        c[2] = (byte)(input[1] & 8 | input[1] << 1 & 6 | input[2] >> 7);
                        c[2] |= (byte)(c[0 * 3 + 2] << 4);
                        c[3] = (byte)(input[2] << 1 & 0xf0 | input[2] >> 3 & 0xf);
                        c[4] = (byte)(input[2] << 5 & 0xe0 | input[3] >> 3 & 0x10);
                        c[4] |= (byte)(c[1 * 3 + 1] >> 4);
                        c[5] = (byte)(input[3] << 1 & 0xf0 | input[3] >> 3 & 0xf);
                    }
                    int di = input[3] & 4 | input[3] << 1 & 2;
                    if (c[0] > c[3] || (c[0] == c[3] && (c[1] > c[4] || (c[1] == c[4] && c[2] >= c[5]))))
                    {
                        ++di;
                    }
                    byte d = ETC.Etc2DistanceTable[di];
                    ETC.ApplicateColor(c, 0, d, color_set);
                    ETC.ApplicateColor(c, 0, -d, color_set[4..]);
                    ETC.ApplicateColor(c, 1, d, color_set[8..]);
                    ETC.ApplicateColor(c, 1, -d, color_set[12..]);
                    for (int i = 0; i < 16; i++, j >>= 1, k >>= 1)
                    {
                        int index = k << 1 & 2 | j & 1;
                        WriteMask(color_set[(index * 4)..], ETC.PunchthroughMaskTable[index], output[(ETC.WriteOrderTable[i] * 4)..]);
                    }
                }
                else
                {
                    int b = input[2] & 0xf8;
                    int db = (input[2] << 3 & 0x18) - (input[2] << 3 & 0x20);
                    if (b + db < 0 || b + db > 255)
                    {
                        // planar (same as Etc2Block)
                        unchecked
                        {
                            c[0] = (byte)(input[0] << 1 & 0xfc | input[0] >> 5 & 3);
                            c[1] = (byte)(input[0] << 7 & 0x80 | input[1] & 0x7e | input[0] & 1);
                            c[2] = (byte)(input[1] << 7 & 0x80 | input[2] << 2 & 0x60 | input[2] << 3 & 0x18 | input[3] >> 5 & 4);
                            c[2] |= (byte)(c[0 * 3 + 2] >> 6);
                            c[3] = (byte)(input[3] << 1 & 0xf8 | input[3] << 2 & 4 | input[3] >> 5 & 3);
                            c[4] = (byte)(input[4] & 0xfe | input[4] >> 7);
                            c[5] = (byte)(input[4] << 7 & 0x80 | input[5] >> 1 & 0x7c);
                            c[5] |= (byte)(c[1 * 3 + 2] >> 6);
                            c[6] = (byte)(input[5] << 5 & 0xe0 | input[6] >> 3 & 0x1c | input[5] >> 1 & 3);
                            c[7] = (byte)(input[6] << 3 & 0xf8 | input[7] >> 5 & 0x6 | input[6] >> 4 & 1);
                            c[8] = (byte)(input[7] << 2 | input[7] >> 4 & 3);
                        }
                        for (int y = 0, i = 0; y < 4; y++)
                        {
                            for (int x = 0; x < 4; x++, i++)
                            {
                                output[i * 4] = ColorConverter.Clamp((x * (c[1 * 3 + 0] - c[0 * 3 + 0]) + y * (c[2 * 3 + 0] - c[0 * 3 + 0]) + 4 * c[0 * 3 + 0] + 2) >> 2);
                                output[i * 4 + 1] = ColorConverter.Clamp((x * (c[1 * 3 + 1] - c[0 * 3 + 1]) + y * (c[2 * 3 + 1] - c[0 * 3 + 1]) + 4 * c[0 * 3 + 1] + 2) >> 2);
                                output[i * 4 + 2] = ColorConverter.Clamp((x * (c[1 * 3 + 2] - c[0 * 3 + 2]) + y * (c[2 * 3 + 2] - c[0 * 3 + 2]) + 4 * c[0 * 3 + 2] + 2) >> 2);
                                output[i * 4 + 3] = byte.MaxValue;
                            }
                        }
                    }
                    else
                    {
                        // differential (Etc1Block + mask + specific mod table)
                        ReadOnlySpan<int> code =
                        [
                            (input[3] >> 5) * 4,
                            (input[3] >> 2 & 7) * 4
                        ];
                        int ti = (input[3] & 1) * 16;
                        unchecked
                        {
                            c[0] = (byte)(r | r >> 5);
                            c[1] = (byte)(g | g >> 5);
                            c[2] = (byte)(b | b >> 5);
                            c[3] = (byte)(r + dr);
                            c[4] = (byte)(g + dg);
                            c[5] = (byte)(b + db);
                            c[3] |= (byte)(c[1 * 3 + 0] >> 5);
                            c[4] |= (byte)(c[1 * 3 + 1] >> 5);
                            c[5] |= (byte)(c[1 * 3 + 2] >> 5);
                        }
                        for (int i = 0; i < 16; i++, j >>= 1, k >>= 1)
                        {
                            int s = ETC.Etc1SubblockTable[ti + i];
                            int index = k << 1 & 2 | j & 1;
                            int ci = code[s];
                            int m = ETC.PunchthroughModifierTable[ci + index];
                            ETC.ApplicateColor(c, s, m, color_set);
                            WriteMask(color_set, ETC.PunchthroughMaskTable[index], output[(ETC.WriteOrderTable[i] * 4)..]);
                        }
                    }
                }
            }
        }


        private static void WriteMask(ReadOnlySpan<byte> data, uint mask, Span<byte> output)
        {
            var temp = BitConverter.GetBytes(mask);
            for (int i = 0; i < temp.Length; i++)
            {
                output[i] = (byte)(data[i] & temp[i]);
            }
        }
    }
}
