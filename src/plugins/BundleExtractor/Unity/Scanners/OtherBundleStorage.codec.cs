using System;
using System.IO;
using YamlDotNet.Core.Tokens;
using ZoDream.BundleExtractor.Models;
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
            if (true)
            {
                return;
            }
            var key = Convert.FromHexString(input.Get<IBundleOptions>().Password);
            if (false)
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
            var version = input.Get<UnityVersion>();
            if (version.Type != UnityVersionType.China || _cipher is null)
            {
                return new BundleBinaryReader(Decode(input.BaseStream, codecType, compressedSize, uncompressedSize), input, false);
            }
            var ms = new MemoryStream();
            _cipher.Decrypt(input.BaseStream, ms);
            return new BundleBinaryReader(ms, input, false); ;
        }

        public Stream Decode(Stream input, BundleCodecType codecType, long compressedSize, long uncompressedSize)
        {
            return BundleCodec.Decode(new PartialStream(input, compressedSize), 
                codecType, uncompressedSize);
        }
    }
}
