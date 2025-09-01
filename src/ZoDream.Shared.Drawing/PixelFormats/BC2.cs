using System;
using System.Buffers.Binary;

namespace ZoDream.Shared.Drawing
{
    public class BC2 : BlockBufferDecoder
    {
        protected override int BlockSize => 16;
        protected override void DecodeBlock(ReadOnlySpan<byte> data, Span<byte> output)
        {
            var destinationPitch = BlockWidth * BlockHeight;
            BC1.ColorBlock(data[8..], output, destinationPitch, 1);
            SharpAlphaBlock(data, output[3..], destinationPitch);
        }

        internal static void SharpAlphaBlock(ReadOnlySpan<byte> compressedBlock, Span<byte> decompressedBlock, int destinationPitch)
        {
            for (int i = 0; i < 4; i ++ )
            {
                ushort alpha = BinaryPrimitives.ReadUInt16LittleEndian(compressedBlock.Slice(i * sizeof(ushort)));
                for (int j = 0; j < 4; j ++)
                {
                    decompressedBlock[j * 4 + i * destinationPitch] = (byte)((((uint)alpha >> (4 * j)) & 0x0F) * 17);
                }
            }
        }
    }
}
