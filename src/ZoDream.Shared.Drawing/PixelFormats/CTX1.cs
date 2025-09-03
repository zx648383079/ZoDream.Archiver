using System;
using System.Runtime.CompilerServices;

namespace ZoDream.Shared.Drawing
{
    public class CTX1(bool swapXY = false, bool computeZ = true) : BlockBufferDecoder
    {
        protected override void DecodeBlock(ReadOnlySpan<byte> data, Span<byte> output)
        {
            Span<byte> vectors = stackalloc byte[4 * 2]; // RG
            vectors[0] = data[1];
            vectors[1] = data[0];

            vectors[2] = data[3];
            vectors[3] = data[2];

            vectors[4] = (byte)((2 * vectors[0] + vectors[2] + 1) / 3);
            vectors[5] = (byte)((2 * vectors[1] + vectors[3] + 1) / 3);

            vectors[6] = (byte)((vectors[0] + 2 * vectors[2] + 1) / 3);
            vectors[7] = (byte)((vectors[1] + 2 * vectors[3] + 1) / 3);

            var code = BitConverter.ToUInt32(data[4..]);

            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    var destIndex = GetBlockIndex(x, y) * PixelSize;
                    var mapIndex = (int)(code & 3) * 2;

                    var r = vectors[mapIndex];
                    var g = vectors[mapIndex + 1];

                    var b = computeZ ? CalculateNormalZ(r, g) : byte.MinValue;

                    if (swapXY)
                    {
                        (r, g) = (g, r);
                    }

                    output[destIndex] = r;
                    output[destIndex + 1] = g;
                    output[destIndex + 2] = b;
                    output[destIndex + 3] = byte.MaxValue;

                    code >>= 2;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static byte CalculateNormalZ(byte r, float g)
        {
            float x = (r / 255f * 2f) - 1f;
            float y = (g / 255f * 2f) - 1f;
            float z = (float)Math.Sqrt(Math.Max(0f, Math.Min(1f, (1f - (x * x)) - (y * y))));
            return (byte)((z + 1f) / 2f * 255f);
        }

   
    }
}
