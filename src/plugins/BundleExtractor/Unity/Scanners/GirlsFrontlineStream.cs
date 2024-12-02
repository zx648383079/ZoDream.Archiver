using System.IO;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal class GirlsFrontlineStream : DeflateStream
    {
        public GirlsFrontlineStream(Stream input)
            : base(input)
        {
            _baseStream = input;
            var originalHeader = new byte[] { 0x55, 0x6E, 0x69, 0x74, 0x79, 0x46, 0x53, 0x00, 0x00, 0x00, 0x00, 0x07, 0x35, 0x2E, 0x78, 0x2E };

            _keys = input.ReadBytes(0x10);
            for (var i = 0; i < _keys.Length; i++)
            {
                var b = (byte)(_keys[i] ^ originalHeader[i]);
                _keys[i] = b != originalHeader[i] ? b : originalHeader[i];
            }
            input.Position = 0;
        }

        private readonly Stream _baseStream;
        private readonly byte[] _keys;

        public override int Read(byte[] buffer, int offset, int count)
        {
            var pos = Position;
            var res = _baseStream.Read(buffer, offset, count);
            for (var i = 0; i < res; i++)
            {
                var j = pos + i;
                if (j >= 0x8000)
                {
                    break;
                }
                buffer[offset + i] ^= _keys[j % _keys.Length];
            }
            return res;
        }
    }
}