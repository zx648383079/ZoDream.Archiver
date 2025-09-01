using System;

namespace ZoDream.Shared.Drawing
{
    public class ETC2A8 : BlockBufferDecoder
    {

        protected override int BlockSize => 16;
        protected override void DecodeBlock(ReadOnlySpan<byte> data, Span<byte> output)
        {
            ETC2.DecodeEtc2Block(data.Slice(8, 8), output);
            DecodeEtc2a8Block(data[..8], output);
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
