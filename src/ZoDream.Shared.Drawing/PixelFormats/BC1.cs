using System;
using System.Buffers.Binary;

namespace ZoDream.Shared.Drawing
{
    /// <summary>
    /// Block Compression
    /// @see https://github.com/AssetRipper/TextureDecoder
    /// DXT1
    /// </summary>
    public class BC1 : BlockBufferDecoder
    {
        protected override void DecodeBlock(ReadOnlySpan<byte> data, Span<byte> output)
        {
            ColorBlock(data, output, BlockWidth * BlockHeight, 0);
        }
        

        internal static void ColorBlock(ReadOnlySpan<byte> compressedBlock, Span<byte> decompressedBlock, 
            int destinationPitch, 
            int onlyOpaqueMode)
        {
            Span<uint> refColors = stackalloc uint[4]; // 0xAABBGGRR

            ushort c0 = BitConverter.ToUInt16(compressedBlock);
            ushort c1 = BitConverter.ToUInt16(compressedBlock[2..]);

            // Expand 565 ref colors to 888 
            uint r0 = (uint)(((((c0 >> 11) & 0x1F) * 527) + 23) >> 6);
            uint g0 = (uint)(((((c0 >> 5) & 0x3F) * 259) + 33) >> 6);
            uint b0 = (uint)((((c0 & 0x1F) * 527) + 23) >> 6);
            refColors[0] = 0xFF000000 | (b0 << 16) | (g0 << 8) | r0;

            uint r1 = (uint)(((((c1 >> 11) & 0x1F) * 527) + 23) >> 6);
            uint g1 = (uint)(((((c1 >> 5) & 0x3F) * 259) + 33) >> 6);
            uint b1 = (uint)((((c1 & 0x1F) * 527) + 23) >> 6);
            refColors[1] = 0xFF000000 | (b1 << 16) | (g1 << 8) | r1;

            uint r;
            uint g;
            uint b;

            if (c0 > c1 || onlyOpaqueMode != 0)
            {
                // Standard BC1 mode (also BC3 color block uses ONLY this mode)
                // color_2 = 2/3*color_0 + 1/3*color_1
                // color_3 = 1/3*color_0 + 2/3*color_1
                r = (((2 * r0) + r1 + 1) / 3);
                g = (((2 * g0) + g1 + 1) / 3);
                b = (((2 * b0) + b1 + 1) / 3);
                refColors[2] = 0xFF000000 | (b << 16) | (g << 8) | r;

                r = ((r0 + (2 * r1) + 1) / 3);
                g = ((g0 + (2 * g1) + 1) / 3);
                b = ((b0 + (2 * b1) + 1) / 3);
                refColors[3] = 0xFF000000 | (b << 16) | (g << 8) | r;
            }
            else
            {
                // Quite rare BC1A mode
                // color_2 = 1/2*color_0 + 1/2*color_1;
                // color_3 = 0;
                r = ((r0 + r1 + 1) >> 1);
                g = ((g0 + g1 + 1) >> 1);
                b = ((b0 + b1 + 1) >> 1);
                refColors[2] = 0xFF000000 | (b << 16) | (g << 8) | r;

                refColors[3] = 0x00000000;
            }

            uint colorIndices = BitConverter.ToUInt16(compressedBlock[4..]);

            // Fill out the decompressed color block 
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    int idx = (int)(colorIndices & 0x03);
                    int decompressedBlockOffset = i * destinationPitch + j * sizeof(uint);
                    if (decompressedBlockOffset + sizeof(uint) > decompressedBlock.Length)
                    {
                        throw new Exception($"Not enough space in decompressed block.\nLength: {decompressedBlock.Length}\nOffset: {decompressedBlockOffset}\nPitch: {destinationPitch}\ni: {i}\nj: {j}");
                    }
                    BinaryPrimitives.WriteUInt32LittleEndian(decompressedBlock[decompressedBlockOffset..], refColors[idx]);
                    colorIndices >>= 2;
                }
            }
        }
    }
}
