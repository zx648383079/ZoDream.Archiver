using System;

namespace ZoDream.Shared.Drawing
{
    /// <summary>
    /// @see https://github.com/AssetRipper/TextureDecoder
    /// </summary>
    public class ETC2 : BlockBufferDecoder
    {
        protected override void DecodeBlock(ReadOnlySpan<byte> data, Span<byte> output)
        {
            DecodeEtc2Block(data, output);
        }

        internal static void DecodeEtc2Block(ReadOnlySpan<byte> data, Span<byte> output)
        {
            int j = data[6] << 8 | data[7];
            int k = data[4] << 8 | data[5];
            Span<byte> c = stackalloc byte[3 * 3];
            Span<byte> color_set = stackalloc byte[4 * 4];
            if ((data[3] & 2) != 0)
            {
                int r = data[0] & 0xf8;
                int dr = (data[0] << 3 & 0x18) - (data[0] << 3 & 0x20);
                if (r + dr < 0 || r + dr > 255)
                {
                    // T
                    unchecked
                    {
                        c[0] = (byte)(data[0] << 3 & 0xc0 | data[0] << 4 & 0x30 | data[0] >> 1 & 0xc | data[0] & 3);
                        c[1] = (byte)(data[1] & 0xf0 | data[1] >> 4);
                        c[2] = (byte)(data[1] & 0x0f | data[1] << 4);
                        c[3] = (byte)(data[2] & 0xf0 | data[2] >> 4);
                        c[4] = (byte)(data[2] & 0x0f | data[2] << 4);
                        c[5] = (byte)(data[3] & 0xf0 | data[3] >> 4);
                    }
                    int ti = data[3] >> 1 & 6 | data[3] & 1;
                    byte d = ETC.Etc2DistanceTable[ti];
                    
                    ETC.ApplicateColorRaw(c, 0, color_set);
                    ETC.ApplicateColor(c, 1, d, color_set[4..]);
                    ETC.ApplicateColorRaw(c, 1, color_set[8..]);
                    ETC.ApplicateColor(c, 1, -d, color_set[12..]);

                    for (int i = 0; i < 16; i++, j >>= 1, k >>= 1)
                    {
                        int index = k << 1 & 2 | j & 1;
                        color_set.Slice(index * 4, 4).CopyTo(output[(ETC.WriteOrderTable[i] * 4)..]);
                    }
                }
                else
                {
                    int g = data[1] & 0xf8;
                    int dg = (data[1] << 3 & 0x18) - (data[1] << 3 & 0x20);
                    if (g + dg < 0 || g + dg > 255)
                    {
                        // H
                        unchecked
                        {
                            c[0] = (byte)(data[0] << 1 & 0xf0 | data[0] >> 3 & 0xf);
                            c[1] = (byte)(data[0] << 5 & 0xe0 | data[1] & 0x10);
                            c[1] |= (byte)(c[0 * 3 + 1] >> 4);
                            c[2] = (byte)(data[1] & 8 | data[1] << 1 & 6 | data[2] >> 7);
                            c[2] |= (byte)(c[0 * 3 + 2] << 4);
                            c[3] = (byte)(data[2] << 1 & 0xf0 | data[2] >> 3 & 0xf);
                            c[4] = (byte)(data[2] << 5 & 0xe0 | data[3] >> 3 & 0x10);
                            c[4] |= (byte)(c[1 * 3 + 1] >> 4);
                            c[5] = (byte)(data[3] << 1 & 0xf0 | data[3] >> 3 & 0xf);
                        }
                        int di = data[3] & 4 | data[3] << 1 & 2;
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
                            color_set.Slice(index * 4, 4).CopyTo(output[(ETC.WriteOrderTable[i] * 4)..]);
                        }
                    }
                    else
                    {
                        int b = data[2] & 0xf8;
                        int db = (data[2] << 3 & 0x18) - (data[2] << 3 & 0x20);
                        if (b + db < 0 || b + db > 255)
                        {
                            // planar
                            unchecked
                            {
                                c[0] = (byte)(data[0] << 1 & 0xfc | data[0] >> 5 & 3);
                                c[1] = (byte)(data[0] << 7 & 0x80 | data[1] & 0x7e | data[0] & 1);
                                c[2] = (byte)(data[1] << 7 & 0x80 | data[2] << 2 & 0x60 | data[2] << 3 & 0x18 | data[3] >> 5 & 4);
                                c[2] |= (byte)(c[0 * 3 + 2] >> 6);
                                c[3] = (byte)(data[3] << 1 & 0xf8 | data[3] << 2 & 4 | data[3] >> 5 & 3);
                                c[4] = (byte)(data[4] & 0xfe | data[4] >> 7);
                                c[5] = (byte)(data[4] << 7 & 0x80 | data[5] >> 1 & 0x7c);
                                c[5] |= (byte)(c[1 * 3 + 2] >> 6);
                                c[6] = (byte)(data[5] << 5 & 0xe0 | data[6] >> 3 & 0x1c | data[5] >> 1 & 3);
                                c[7] = (byte)(data[6] << 3 & 0xf8 | data[7] >> 5 & 0x6 | data[6] >> 4 & 1);
                                c[8] = (byte)(data[7] << 2 | data[7] >> 4 & 3);
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
                            // differential
                            ReadOnlySpan<int> code =
                            [
                                (data[3] >> 5) * 4,
                                (data[3] >> 2 & 7) * 4
                            ];
                            int ti = (data[3] & 1) * 16;
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
                                int m = ETC.Etc1ModifierTable[ci + index];
                                ETC.ApplicateColor(c, s, m, output[(ETC.WriteOrderTable[i] * 4)..]);
                            }
                        }
                    }
                }
            }
            else
            {
                // individual
                ReadOnlySpan<int> code =
                [
                    (data[3] >> 5) * 4,
                    (data[3] >> 2 & 7) * 4
                ];
                int ti = (data[3] & 1) * 16;
                unchecked
                {
                    c[0] = (byte)(data[0] & 0xf0 | data[0] >> 4);
                    c[3] = (byte)(data[0] & 0x0f | data[0] << 4);
                    c[1] = (byte)(data[1] & 0xf0 | data[1] >> 4);
                    c[4] = (byte)(data[1] & 0x0f | data[1] << 4);
                    c[2] = (byte)(data[2] & 0xf0 | data[2] >> 4);
                    c[5] = (byte)(data[2] & 0x0f | data[2] << 4);
                }
                for (int i = 0; i < 16; i++, j >>= 1, k >>= 1)
                {
                    int s = ETC.Etc1SubblockTable[ti + i];
                    int index = k << 1 & 2 | j & 1;
                    int ci = code[s];
                    int m = ETC.Etc1ModifierTable[ci + index];
                    ETC.ApplicateColor(c, s, m, output[(ETC.WriteOrderTable[i] * 4)..]);
                }
            }
        }

       
    }
}
