using System.IO;
using ZoDream.ChmExtractor.Lzx;

namespace ZoDream.ChmExtractor
{
    public static class LzxCodec
    {
        public static void Decode(Stream input, Stream output)
        {
            var decoder = new LzxDecoder(16);
            var seekPos = input.Position;
            while (input.Position - 14 < input.Length)
            {
                input.Seek(seekPos, SeekOrigin.Begin);

                var a = input.ReadByte();
                var b = input.ReadByte();
                seekPos += 2;

                var chunk = 0x8000;
                var block = (a << 8) | b;

                if (a == 0xFF)
                {
                    chunk = (b << 8) | input.ReadByte();
                    block = (input.ReadByte() << 8) | input.ReadByte();
                    seekPos += 3;
                }


                if (chunk == 0 || block == 0)
                {
                    break;
                }

                var err = decoder.Decompress(input, block, output, chunk);
                if (err != 0)
                {
                    break;
                }

                seekPos += block;
            }
        }
    }
}
