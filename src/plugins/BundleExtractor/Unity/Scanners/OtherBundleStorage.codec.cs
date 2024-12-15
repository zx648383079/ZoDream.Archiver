using System;
using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class OtherBundleElementScanner
    {

        private IDecryptCipher? _cipher;

        public void Initialize(IBundleBinaryReader input) 
        {
            if (!IsChina || string.IsNullOrWhiteSpace(options.Password))
            {
                return;
            }
            var key = Convert.FromHexString(options.Password);
            if (IsGuiLongChao)
            {
                _cipher = new UnityCNGuiLongChao(input)
                {
                    Key = key
                };
            } else
            {
                _cipher = new CNCipher(input)
                {
                    Key = key
                };
            }
        }
        public IBundleBinaryReader Decode(IBundleBinaryReader input, BundleCodecType codecType, long compressedSize, long uncompressedSize)
        {
            return new BundleBinaryReader(
                BundleCodec.Decode(
                    new PartialStream(input.BaseStream, compressedSize), 
                    codecType, uncompressedSize), input, false);
        }

        public Stream Decode(Stream input, BundleCodecType codecType, long compressedSize, long uncompressedSize)
        {
            if (_cipher is null)
            {
                return BundleCodec.Decode(
                    new PartialStream(input, compressedSize), codecType, uncompressedSize);
            } else
            {
                var ms = new MemoryStream();
                _cipher.Decrypt(input, ms);
                return ms;
            }
        }
    }
}
