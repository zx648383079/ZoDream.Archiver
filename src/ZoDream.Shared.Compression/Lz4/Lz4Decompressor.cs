using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;

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
            return Decompress(input, 0, inputLength, output, outputLength);
        }
        public int Decompress(byte[] input, int inputIndex, int inputLength, 
            byte[] output, int outputLength)
        {
            return Decompress(new MemoryStream(input, inputIndex, inputLength), output, outputLength);
        }

        public int Decompress(Stream input,
            byte[] output, int outputLength)
        {
            var outputPos = 0;
            do
            {
                var (encCount, litCount) = GetLiteralToken(input);

                //Copy literal chunk
                litCount = GetLength(litCount, input);
                input.ReadExactly(output, outputPos, litCount);
                outputPos += litCount;

                if (input.Position >= input.Length)
                {
                    break;
                }

                //Copy compressed chunk
                int back = GetChunkEnd(input);

                encCount = GetLength(encCount, input) + 4;

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
            } while (input.Position < input.Length && outputPos < outputLength);
            Debug.Assert(outputPos == outputLength);
            return outputPos;
        }

        public void Decompress(Stream input, Stream output)
        {
            if (output is ArrayMemoryStream ms)
            {
                ms.Position = Decompress(input, ms.GetBuffer(), (int)Math.Min(unCompressedSize, ms.Length));
                return;
            }
            var buffer = ArrayPool<byte>.Shared.Rent((int)unCompressedSize);
            try
            {
                var res = Decompress(input, buffer, (int)unCompressedSize);
                output.Write(buffer, 0, res);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        protected virtual (int encCount, int litCount) GetLiteralToken(Stream input)
        {
            var b = (byte)input.ReadByte();
            return ((b >> 0) & 0xf, (b >> 4) & 0xf);
        }
        protected virtual int GetChunkEnd(Stream input)
        {
            return input.ReadByte() << 0 | input.ReadByte() << 8;
        }
        protected virtual int GetLength(int length,
            Stream input)
        {
            byte sum;

            if (length == 0xf)
            {
                do
                {
                    length += sum = (byte)input.ReadByte();
                } while (sum == 0xff);
            }
            return length;
        }
    }
}
