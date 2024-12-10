using System;
using System.IO;
using System.Linq;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;

namespace ZoDream.Shared.Compression.Lz4
{
    public class Lz4Decompressor(long unCompressedSize) : IDecompressor
    {
        public byte[] Decompress(byte[] input)
        {
            var output = new byte[unCompressedSize];
            var outputPos = Decompress(input, output);
            return outputPos == output.Length ? output : output[..outputPos];
        }

        public int Decompress(byte[] input, byte[] output)
        {
            int inputPos = 0;
            int outputPos = 0;
            do
            {
                var (encCount, litCount) = GetLiteralToken(input, ref inputPos);

                //Copy literal chunk
                litCount = GetLength(litCount, input, ref inputPos);
                Array.Copy(input, inputPos, output, outputPos, litCount);
                inputPos += litCount;
                outputPos += litCount;

                if (inputPos >= input.Length)
                {
                    break;
                }

                //Copy compressed chunk
                int back = GetChunkEnd(input, ref inputPos);

                encCount = GetLength(encCount, input, ref inputPos) + 4;

                int encPos = outputPos - back;

                if (encCount <= back)
                {
                    Array.Copy(input, encPos, output, outputPos, encCount);
                    outputPos += encCount;
                }
                else
                {
                    while (encCount-- > 0)
                    {
                        output[outputPos++] = output[encPos++];
                    }
                }
            } while (inputPos < input.Length && outputPos < output.Length);

            return outputPos;
        }

        public void Decompress(Stream input, Stream output)
        {
            output.Write(Decompress(input.ToArray()));
        }

        protected virtual (int encCount, int litCount) GetLiteralToken(byte[] input, ref int inputPos) => ((input[inputPos] >> 0) & 0xf, (input[inputPos++] >> 4) & 0xf);
        protected virtual int GetChunkEnd(byte[] input, ref int inputPos) => input[inputPos++] << 0 | input[inputPos++] << 8;
        protected virtual int GetLength(int length, 
            byte[] input, ref int inputPos)
        {
            byte sum;

            if (length == 0xf)
            {
                do
                {
                    length += sum = input[inputPos++];
                } while (sum == 0xff);
            }
            return length;
        }
    }
}
