using System;

namespace ZoDream.Shared.Drawing
{
    /// <summary>
    /// BC3
    /// </summary>
    public class DXT5 : BlockBufferDecoder
    {
        protected override int BlockSize => 16;

        protected override void DecodeBlock(ReadOnlySpan<byte> data, Span<byte> output)
        {
            uint[] alphas = new uint[8];
            alphas[0] = data[0];
            alphas[1] = data[1];
            ulong alphaMask = BitConverter.ToUInt64(data) >> 16;

            

            if (alphas[0] > alphas[1])
            {
                alphas[2] = (byte)((6 * alphas[0] + 1 * alphas[1] + 3) / 7);
                alphas[3] = (byte)((5 * alphas[0] + 2 * alphas[1] + 3) / 7);
                alphas[4] = (byte)((4 * alphas[0] + 3 * alphas[1] + 3) / 7);
                alphas[5] = (byte)((3 * alphas[0] + 4 * alphas[1] + 3) / 7);
                alphas[6] = (byte)((2 * alphas[0] + 5 * alphas[1] + 3) / 7);
                alphas[7] = (byte)((1 * alphas[0] + 6 * alphas[1] + 3) / 7);
            }
            else
            {
                alphas[2] = (byte)((4 * alphas[0] + 1 * alphas[1] + 2) / 5);
                alphas[3] = (byte)((3 * alphas[0] + 2 * alphas[1] + 2) / 5);
                alphas[4] = (byte)((2 * alphas[0] + 3 * alphas[1] + 2) / 5);
                alphas[5] = (byte)((1 * alphas[0] + 4 * alphas[1] + 2) / 5);
                alphas[6] = byte.MinValue;
                alphas[7] = byte.MaxValue;
            }

            var alpha = new byte[4, 4];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    alpha[j, i] = (byte)alphas[alphaMask & 7];
                    alphaMask >>= 3;
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
                                output[pixDataStart] = byte.MinValue;
                                output[pixDataStart + 1] = byte.MinValue;
                                output[pixDataStart + 2] = byte.MinValue;
                            }
                            break;
                    }

                    code >>= 2;
                }
            }
        }

     
    }
}
