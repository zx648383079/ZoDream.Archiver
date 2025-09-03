using System;
using System.Buffers.Binary;

namespace ZoDream.Shared.Drawing
{
    /// <summary>
    /// @see https://github.com/AssetRipper/TextureDecoder
    /// DXT5
    /// </summary>
    public class BC3 : BlockBufferDecoder
    {
        protected override int BlockSize => 16;
        protected override void DecodeBlock(ReadOnlySpan<byte> data, Span<byte> output)
        {
            var destinationPitch = BlockWidth * BlockHeight;
            BC1.ColorBlock(data[8..], output, destinationPitch, 1);
            SmoothAlphaBlock(data, output[3..], destinationPitch, 4);
        }

        internal static void SmoothAlphaBlock(ReadOnlySpan<byte> compressedBlock, Span<byte> decompressedBlock, int destinationPitch, int pixelSize)
        {
            Span<byte> alpha = stackalloc byte[8];

            ulong block = BinaryPrimitives.ReadUInt64LittleEndian(compressedBlock);

            alpha[0] = (byte)(block & 0xFF);
            alpha[1] = (byte)((block >> 8) & 0xFF);

            if (alpha[0] > alpha[1])
            {
                // 6 interpolated alpha values. 
                alpha[2] = (byte)(((6 * alpha[0]) + alpha[1] + 1) / 7); // 6/7*alpha_0 + 1/7*alpha_1
                alpha[3] = (byte)(((5 * alpha[0]) + (2 * alpha[1]) + 1) / 7); // 5/7*alpha_0 + 2/7*alpha_1
                alpha[4] = (byte)(((4 * alpha[0]) + (3 * alpha[1]) + 1) / 7); // 4/7*alpha_0 + 3/7*alpha_1
                alpha[5] = (byte)(((3 * alpha[0]) + (4 * alpha[1]) + 1) / 7); // 3/7*alpha_0 + 4/7*alpha_1
                alpha[6] = (byte)(((2 * alpha[0]) + (5 * alpha[1]) + 1) / 7); // 2/7*alpha_0 + 5/7*alpha_1
                alpha[7] = (byte)((alpha[0] + (6 * alpha[1]) + 1) / 7); // 1/7*alpha_0 + 6/7*alpha_1
            }
            else
            {
                // 4 interpolated alpha values. 
                alpha[2] = (byte)(((4 * alpha[0]) + alpha[1] + 1) / 5); // 4/5*alpha_0 + 1/5*alpha_1
                alpha[3] = (byte)(((3 * alpha[0]) + (2 * alpha[1]) + 1) / 5); // 3/5*alpha_0 + 2/5*alpha_1
                alpha[4] = (byte)(((2 * alpha[0]) + (3 * alpha[1]) + 1) / 5); // 2/5*alpha_0 + 3/5*alpha_1
                alpha[5] = (byte)((alpha[0] + (4 * alpha[1]) + 1) / 5); // 1/5*alpha_0 + 4/5*alpha_1
                alpha[6] = 0x00;
                alpha[7] = 0xFF;
            }

            ulong indices = (ulong)(block >> 16);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    decompressedBlock[j * pixelSize + i * destinationPitch] = alpha[(int)(indices & 0x07)];
                    indices >>= 3;
                }
            }
        }
    }
}
