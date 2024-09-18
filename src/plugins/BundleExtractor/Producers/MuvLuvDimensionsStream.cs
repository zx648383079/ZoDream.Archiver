using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Producers
{
    public class MuvLuvDimensionsStream(Stream stream): DeflateStream(stream)
    {

        private readonly byte[] _keys = [0xFD, 0x13, 0x7B, 0xEE, 0xC5, 0xFE, 0x50, 0x12, 0x4D, 0x38];

        public override int Read(byte[] buffer, int offset, int count)
        {
            var pos = Position;
            var res = stream.Read(buffer, offset, count);
            for (var i = 0; i < res; i++)
            {
                var j = pos + i;
                buffer[i] ^= _keys[j % _keys.Length];
            }
            return res;
        }
    }
}
