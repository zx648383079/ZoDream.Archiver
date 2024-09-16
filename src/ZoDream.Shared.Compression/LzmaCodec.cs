using SharpCompress.Compressors.LZMA;
using System.IO;

namespace ZoDream.Shared.Compression
{
    public static class LzmaCodec
    {
        private const int PropertiesSize = 5;

        public static Stream Decode(Stream input, long outputLength)
        {
            var properties = new byte[PropertiesSize];
            input.ReadExactly(properties, 0, PropertiesSize);
            var headlessSize = input.Length - input.Position;
            return new LzmaStream(properties, input, headlessSize, -1, null, false);
        }
    }
}
