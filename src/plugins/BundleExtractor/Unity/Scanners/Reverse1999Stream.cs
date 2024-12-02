using System.IO;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal class Reverse1999Stream(Stream input, byte key)
        : DeflateStream(input)
    {
        public override int Read(byte[] buffer, int offset, int count)
        {
            var res = input.Read(buffer, offset, count);
            for (var i = 0; i < res; i++)
            {
                buffer[offset + i] ^= key;
            }
            return res;
        }
    }
}