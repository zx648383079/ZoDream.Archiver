using System;

namespace ZoDream.Shared.Drawing
{
    public class EACRGUnsigned : BlockBufferDecoder
    {
        protected override int BlockSize => 16;

        protected override void DecodeBlock(ReadOnlySpan<byte> data, Span<byte> output)
        {
            DecodeEacUnsignedBlock(data[..8], output, 2);
            DecodeEacUnsignedBlock(data.Slice(8, 8), output, 1);
        }

        private static void DecodeEacUnsignedBlock(ReadOnlySpan<byte> input, Span<byte> output, int channel)
        {
            int @base = input[0];
            int data1 = input[1];
            int mul = data1 >> 4;
            if (mul == 0)
            {
                EACRSigned.DecodeEac11Block(output[channel..], @base);
            }
            else
            {
                int table = data1 & 0xF;
                ulong l = ColorConverter.ReadUInt64From6Bit(input[2..]);
                EACRSigned.DecodeEac11Block(output[channel..], @base, table, mul, l);
            }
        }

        
    }
}
