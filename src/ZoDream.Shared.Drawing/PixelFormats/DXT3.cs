using System;

namespace ZoDream.Shared.Drawing
{
    public class DXT3 : BlockBufferDecoder
    {
        protected override int BlockSize => 16;
        protected override void DecodeBlock(ReadOnlySpan<byte> data, Span<byte> output)
        {
            var alphaData = new ushort[4];

            alphaData[0] = BitConverter.ToUInt16(data);
            alphaData[1] = BitConverter.ToUInt16(data[2..]);
            alphaData[2] = BitConverter.ToUInt16(data[4..]);
            alphaData[3] = BitConverter.ToUInt16(data[6..]);

            var alpha = new byte[4, 4];
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    alpha[i, j] = (byte)((alphaData[j] & 0xF) * 16);
                    alphaData[j] >>= 4;
                }
            }

            ushort color0 = BitConverter.ToUInt16(data[8..]);
            ushort color1 = BitConverter.ToUInt16(data[10..]);

            uint code = BitConverter.ToUInt32(data[12..]);

            var r0 = (ushort)(8 * (color0 & 31));
            var g0 = (ushort)(4 * ((color0 >> 5) & 63));
            var b0 = (ushort)(8 * ((color0 >> 11) & 31));

            var r1 = (ushort)(8 * (color1 & 31));
            var g1 = (ushort)(4 * ((color1 >> 5) & 63));
            var b1 = (ushort)(8 * ((color1 >> 11) & 31));

            for (int k = 0; k < 4; k++)
            {
                int y = k ^ 1;
                for (int x = 0; x < 4; x++)
                {
                    int pixDataStart = GetBlockIndex(x, y) * 4;
                    uint codeDec = code & 0x3;

                    output[pixDataStart + 3] = alpha[x, y];

                    switch (codeDec)
                    {
                        case 0:
                            output[pixDataStart] = (byte)r0;
                            output[pixDataStart + 1] = (byte)g0;
                            output[pixDataStart + 2] = (byte)b0;
                            break;
                        case 1:
                            output[pixDataStart] = (byte)r1;
                            output[pixDataStart + 1] = (byte)g1;
                            output[pixDataStart + 2] = (byte)b1;
                            break;
                        case 2:
                            if (color0 > color1)
                            {
                                output[pixDataStart] = (byte)((2 * r0 + r1) / 3);
                                output[pixDataStart + 1] = (byte)((2 * g0 + g1) / 3);
                                output[pixDataStart + 2] = (byte)((2 * b0 + b1) / 3);
                            }
                            else
                            {
                                output[pixDataStart] = (byte)((r0 + r1) / 2);
                                output[pixDataStart + 1] = (byte)((g0 + g1) / 2);
                                output[pixDataStart + 2] = (byte)((b0 + b1) / 2);
                            }
                            break;
                        case 3:
                            if (color0 > color1)
                            {
                                output[pixDataStart] = (byte)((r0 + 2 * r1) / 3);
                                output[pixDataStart + 1] = (byte)((g0 + 2 * g1) / 3);
                                output[pixDataStart + 2] = (byte)((b0 + 2 * b1) / 3);
                            }
                            else
                            {
                                output[pixDataStart] = 0;
                                output[pixDataStart + 1] = 0;
                                output[pixDataStart + 2] = 0;
                            }
                            break;
                    }

                    code >>= 2;
                }
            }
        }

    }
}
