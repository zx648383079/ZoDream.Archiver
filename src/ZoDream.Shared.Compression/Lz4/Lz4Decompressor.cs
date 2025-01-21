using System;
using System.Buffers;
using System.IO;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Compression.Lz4
{
    public class Lz4Decompressor(long unCompressedSize) : IDecompressor
    {
        public byte[] Decompress(byte[] input)
        {
            return Decompress(input, input.Length);
        }
        public byte[] Decompress(byte[] input, int inputLength)
        {
            var output = new byte[unCompressedSize];
            var outputPos = Decompress(input, inputLength, output, (int)unCompressedSize);
            return outputPos == output.Length ? output : output[..outputPos];
        }

        public int Decompress(byte[] input, int inputLength, byte[] output, int outputLength)
        {
            var inputPos = 0;
            var outputPos = 0;
            do
            {
                var (encCount, litCount) = GetLiteralToken(input, ref inputPos);

                //Copy literal chunk
                litCount = GetLength(litCount, input, ref inputPos);
                Array.Copy(input, inputPos, output, outputPos, litCount);
                inputPos += litCount;
                outputPos += litCount;

                if (inputPos >= inputLength)
                {
                    break;
                }

                //Copy compressed chunk
                int back = GetChunkEnd(input, ref inputPos);

                encCount = GetLength(encCount, input, ref inputPos) + 4;

                int encPos = outputPos - back;
                if (encCount <= back)
                {
                    Array.Copy(output, encPos, output, outputPos, encCount);
                    outputPos += encCount;
                }
                else
                {
                    while (encCount-- > 0)
                    {
                        output[outputPos++] = output[encPos++];
                    }
                }
            } while (inputPos < inputLength && outputPos < outputLength);

            return outputPos;
        }

        public void Decompress(Stream input, Stream output)
        {
            var length = (int)(input.Length - input.Position);
            var buffer = ArrayPool<byte>.Shared.Rent(length);
            var outputBuffer = ArrayPool<byte>.Shared.Rent((int)unCompressedSize);
            try
            {
                input.ReadExactly(buffer, 0, length);
                var res = Decompress(buffer, length, outputBuffer, (int)unCompressedSize);
                output.Write(outputBuffer, 0, res);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(outputBuffer);
                ArrayPool<byte>.Shared.Return(buffer);
            }
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
