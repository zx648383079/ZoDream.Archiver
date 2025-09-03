using System;
using System.Runtime.CompilerServices;

namespace ZoDream.Shared.Drawing
{
    /// <summary>
    /// @see https://github.com/AssetRipper/TextureDecoder
    /// </summary>
    internal class EACRSigned : BlockBufferDecoder
    {
        protected override void DecodeBlock(ReadOnlySpan<byte> data, Span<byte> output)
        {
            DecodeEacSignedBlock(data, output, 2);
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
