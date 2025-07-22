using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class CiPaiBundleElementScanner : IBundleCodec
    {
        public IBundleBinaryReader Decode(IBundleBinaryReader input, BundleCodecType codecType, long compressedSize, long uncompressedSize)
        {
            return new BundleBinaryReader(
                Decode(input.BaseStream, codecType, compressedSize, uncompressedSize), input, false);
        }

        public Stream Decode(Stream input, BundleCodecType codecType, long compressedSize, long uncompressedSize)
        {
            var res = new PartialStream(input, compressedSize);
            if (IsPerpetualNovelty && codecType is BundleCodecType.Lz4 or BundleCodecType.Lz4HC)
            {
                res.Position++;
                var key = res.ReadByte();
                res.Seek(0, SeekOrigin.Begin);
                return new XORStream(res, [(byte)key], 71);
            }
            return res;
        }

        public void Initialize(IBundleBinaryReader input)
        {
        }
    }
}
