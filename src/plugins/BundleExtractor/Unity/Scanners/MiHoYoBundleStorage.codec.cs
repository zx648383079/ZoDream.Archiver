using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class MiHoYoElementScanner : IBundleCodec
    {

        private IDecryptCipher? _cipher;

        public void Initialize(IBundleBinaryReader input)
        {
            _cipher = new Mr0kCipher();
        }

        public IBundleBinaryReader Decode(IBundleBinaryReader input, BundleCodecType codecType, long compressedSize, long uncompressedSize)
        {
            Stream stream = new PartialStream(input.BaseStream, compressedSize);
            if (_cipher is not null && codecType == BundleCodecType.Lz4Mr0k)
            {
                var ms = new MemoryStream();
                _cipher.Decrypt(stream, ms);
                stream = ms;
            }
            return new BundleBinaryReader(
                BundleCodec.Decode(
                    stream,
                    codecType, uncompressedSize), input, false);
        }

        public Stream Decode(Stream input, BundleCodecType codecType, long compressedSize, long uncompressedSize)
        {
            Stream stream = new PartialStream(input, compressedSize);
            if (_cipher is not null && codecType == BundleCodecType.Lz4Mr0k)
            {
                var ms = new MemoryStream();
                _cipher.Decrypt(stream, ms);
                stream = ms;
            }
            return BundleCodec.Decode(
                    stream, codecType, uncompressedSize);
        }
    }
}
