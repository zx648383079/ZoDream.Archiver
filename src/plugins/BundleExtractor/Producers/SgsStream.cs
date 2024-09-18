using System.IO;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Producers
{
    public class SgsStream(Stream stream) : DeflateStream(stream)
    {

        private readonly int _stride = (int)(stream.Length % 7 + 3);
        private byte _key = 0xFF;

        public override int Read(byte[] buffer, int offset, int count)
        {
            var pos = Position;
            var res = stream.Read(buffer, offset, count);
            for (var i = 0; i < res; i++)
            {
                var j = pos + i;
                if (j % _stride != 0)
                {
                    buffer[i] ^= _key;
                    continue;
                }
                _key = (byte)~_key;
            }
            return res;
        }
    }
}
