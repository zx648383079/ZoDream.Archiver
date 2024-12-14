using LzhamWrapper;
using LzhamWrapper.Enums;
using System.Buffers;
using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Compression;
using ZoDream.Shared.Compression.Lz4;
using ZoDream.Shared.Exceptions;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor
{
    public class BundleCodec : IBundleCodec
    {
        public void Initialize(IBundleBinaryReader input) { }
        public IBundleBinaryReader Decode(IBundleBinaryReader input, BundleCodecType type, long compressedSize, long uncompressedSize)
        {

            return new BundleBinaryReader(Decode(input.BaseStream, type, compressedSize, uncompressedSize), input, false);
        }

        public Stream Decode(Stream input, BundleCodecType type, long compressedSize, long uncompressedSize)
        {
            var ms = new PartialStream(input, compressedSize);
            return Decode(ms, type, uncompressedSize);
        }

        public static Stream Decode(Stream input, BundleCodecType type, long uncompressedSize)
        {
            switch (type)
            {
                case BundleCodecType.Lzma:
                    return LzmaCodec.Decode(input, uncompressedSize);
                case BundleCodecType.Lz4:
                case BundleCodecType.Lz4HC:
                    return DecodeLz4(input, (int)uncompressedSize);
                case BundleCodecType.Lzham:
                    return DecodeLzham(input);
                default:
                    return input;
            }
        }

        public static Stream DecodeLz4(Stream input, int uncompressedSize)
        {
            var compressedLength = (int)(input.Length - input.Position);
            var compressedBytes = ArrayPool<byte>.Shared.Rent(compressedLength);
            try
            {
                input.ReadExactly(compressedBytes, 0, compressedLength);
                var uncompressedBytes = new byte[uncompressedSize];
                var bytesWritten = 0; //LZ4Codec.Decode(compressedBytes, 0, compressedLength, uncompressedBytes, 0, uncompressedSize);
                if (bytesWritten != uncompressedSize)
                {
                    bytesWritten = new Lz4Decompressor(uncompressedSize)
                        .Decompress(compressedBytes, compressedLength,
                        uncompressedBytes, uncompressedSize);
                    if (bytesWritten != uncompressedSize)
                    {
                        throw new DecompressionFailedException($"lz4 wants: {uncompressedSize} not {bytesWritten}");
                    }
                }
                return new MemoryStream(uncompressedBytes);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(compressedBytes);
            }
        }
        public static Stream DecodeLzham(Stream input)
        {
            var opts = new DecompressionParameters
            {
                DictionarySize = 26,
                UpdateRate = TableUpdateRate.Default,
            };
            //opts.Flags |= Zlib ? DecompressionFlag.ReadZlibStream : 0;
            //opts.Flags |= Unbuffered ? DecompressionFlag.OutputUnbuffered : 0;
            //opts.Flags |= NoAdler ? 0 : DecompressionFlag.ComputeAdler32;
            return new LzhamStream(input, opts);
        }
    }
}
