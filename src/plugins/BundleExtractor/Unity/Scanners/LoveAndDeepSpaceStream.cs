using System.IO;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public class LoveAndDeepSpaceStream: DeflateStream
    {
        public LoveAndDeepSpaceStream(Stream input): base(input)
        {
            var vector = new byte[] { 0x35, 0x6B, 0x05, 0x00 };
            var originalHeader = new byte[] { 0x55, 0x6E, 0x69, 0x74, 0x79, 0x46, 0x53, 0x00, 0x00, 0x00, 0x00, 0x07, 0x35, 0x2E, 0x78, 0x2E };

            var seed = new byte[0x10]; 
            input.ReadExactly(seed);
            for (int i = 0; i < seed.Length; i++)
            {
                var b = (byte)(seed[i] ^ originalHeader[i] ^ vector[0]);
                seed[i] = b != originalHeader[i] ? b : originalHeader[i];
            }

            for (int i = 0; i < vector.Length; i++)
            {
                for (int j = 0; j < seed.Length; j++)
                {
                    var offset = i * 0x10;
                    _keys[offset + j] = (byte)(seed[j] ^ vector[i]);
                }
            }
            input.Position = 0;
            _baseStream = input;
        }

        private readonly Stream _baseStream;

        private readonly byte[] _keys = new byte[0x40];

        public override int Read(byte[] buffer, int offset, int count)
        {
            var pos = Position;
            var res = _baseStream.Read(buffer, offset, count);
            for (var i = 0; i < res; i++)
            {
                var j = pos + i;
                buffer[offset + i] ^= _keys[j % _keys.Length];
            }
            return res;
        }
    }
}
