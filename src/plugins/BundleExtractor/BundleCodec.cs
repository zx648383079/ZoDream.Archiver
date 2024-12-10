using K4os.Compression.LZ4;
using LzhamWrapper;
using LzhamWrapper.Enums;
using System.IO;
using System.Linq;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Compression;
using ZoDream.Shared.Compression.Lz4;
using ZoDream.Shared.Exceptions;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor
{
    public static class BundleCodec
    {

        public static IBundleBinaryReader Decode(IBundleBinaryReader input, CompressionType type, long compressedSize, long uncompressedSize)
        {
            return new BundleBinaryReader(Decode(input.BaseStream, type, compressedSize, uncompressedSize), input, false);
        }

        public static Stream Decode(Stream input, CompressionType type, long compressedSize, long uncompressedSize)
        {
            var ms = new PartialStream(input, compressedSize);
            return Decode(ms, type, uncompressedSize);
        }
        public static Stream Decode(Stream input, CompressionType type, long uncompressedSize)
        {
            switch (type)
            {
                case CompressionType.Lzma:
                    return LzmaCodec.Decode(input, uncompressedSize);

                case CompressionType.Lz4:
                case CompressionType.Lz4HC:
                    {
                        var compressedBytes = input.ToArray();
                        var uncompressedBytes = new byte[uncompressedSize];
                        var bytesWritten = LZ4Codec.Decode(compressedBytes, uncompressedBytes);
                        if (bytesWritten != uncompressedSize)
                        {
                            bytesWritten = new Lz4Decompressor(uncompressedSize)
                                .Decompress(compressedBytes, uncompressedBytes);
                            if (bytesWritten != uncompressedSize)
                            {
                                throw new DecompressionFailedException($"{type} wants: {uncompressedSize} not {bytesWritten}");
                            }
                        }
                        
                        return new MemoryStream(uncompressedBytes);
                    }

                case CompressionType.Lzham:
                    return DecodeLzham(input);
                default:
                    return input;
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
