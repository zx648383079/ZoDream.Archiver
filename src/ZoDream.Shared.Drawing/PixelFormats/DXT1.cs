using System;

namespace ZoDream.Shared.Drawing
{
    /// <summary>
    /// S3 Texture Compression
    /// BC1
    /// </summary>
    public class DXT1 : BlockBufferDecoder
    {
        protected override void DecodeBlock(ReadOnlySpan<byte> data, Span<byte> output)
        {
            uint colour0 = BitConverter.ToUInt16(data);
            uint colour1 = BitConverter.ToUInt16(data[2..]);
            uint code = BitConverter.ToUInt32(data[4..]);


            var r0 = (ushort)(8 * (colour0 & 0x1F));
            var g0 = (ushort)(4 * ((colour0 >> 5) & 0x3F));
            var b0 = (ushort)(8 * ((colour0 >> 11) & 0x1F));

            var r1 = (ushort)(8 * (colour1 & 0x1F));
            var g1 = (ushort)(4 * ((colour1 >> 5) & 0x3F));
            var b1 = (ushort)(8 * ((colour1 >> 11) & 0x1F));

            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    int dataStart = GetBlockIndex(x, y) * PixelSize;
                    switch (code & 3)
                    {
                        case 0:
                            output[dataStart] = (byte)r0;
                            output[dataStart + 1] = (byte)g0;
                            output[dataStart + 2] = (byte)b0;
                            output[dataStart + 3] = byte.MaxValue;
                            break;

                        case 1:
                            output[dataStart] = (byte)r1;
                            output[dataStart + 1] = (byte)g1;
                            output[dataStart + 2] = (byte)b1;
                            output[dataStart + 3] = byte.MaxValue;
                            break;

                        case 2:
                            output[dataStart + 3] = byte.MaxValue;
                            if (colour0 <= colour1)
                            {
                                output[dataStart] = (byte)((r0 + r1) / 2);
                                output[dataStart + 1] = (byte)((g0 + g1) / 2);
                                output[dataStart + 2] = (byte)((b0 + b1) / 2);
                            }
                            output[dataStart] = (byte)(((2 * r0) + r1) / 3);
                            output[dataStart + 1] = (byte)(((2 * g0) + g1) / 3);
                            output[dataStart + 2] = (byte)(((2 * b0) + b1) / 3);
                            break;

                        case 3:
                            if (colour0 <= colour1)
                            {
                                output[dataStart] = byte.MinValue;
                                output[dataStart + 1] = byte.MinValue;
                                output[dataStart + 2] = byte.MinValue;
                                output[dataStart + 3] = byte.MinValue;
                            }
                            output[dataStart] = (byte)((r0 + (2 * r1)) / 3);
                            output[dataStart + 1] = (byte)((g0 + (2 * g1)) / 3);
                            output[dataStart + 2] = (byte)((b0 + (2 * b1)) / 3);
                            output[dataStart + 3] = byte.MaxValue;
                            break;

                        default:
                            break;
                    }
                    code >>= 2;
                }
            }
        }

        
    }
}
