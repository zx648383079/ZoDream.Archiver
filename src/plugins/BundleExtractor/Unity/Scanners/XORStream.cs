using System.IO;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal class XORStream(
        Stream input,
        byte[] xorPad) : DeflateStream(input)
    {
        public override int Read(byte[] buffer, int offset, int count)
        {
            var pos = Position;
            var read = input.Read(buffer, offset, count);
            for (var i = 0; i < read; i++)
            {
                buffer[offset + i] ^= xorPad[(pos + i) % xorPad.Length];
            }
            return read;
        }
    }
}
