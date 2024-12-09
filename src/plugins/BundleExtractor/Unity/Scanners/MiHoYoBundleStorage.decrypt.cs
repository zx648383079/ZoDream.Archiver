using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using ZoDream.BundleExtractor.Unity.BundleFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class MiHoYoElementScanner
    {

        private Stream DecryptPack(Stream input)
        {
            const int PackSize = 0x880;
            const string PackSignature = "pack";

            var finder = new StreamFinder(FileStreamBundleHeader.UnityFSMagic)
            {
                IsMatchFirst = true,
                MaxPosition = 1024,
            };
            if (!finder.MatchFile(input))
            {
                input.Position = 0;
                return input;
            }
            finder = new StreamFinder("mr0k")
            {
                IsMatchFirst = true,
                MaxPosition = 1024,
            };
            if (!finder.MatchFile(input))
            {
                input.Position = 0;
                return input;
            }
            var ms = new MemoryStream();
            // TODO 配置密钥
            var cipher = new Mr0kCipher();
            try
            {
                long readSize = 0;
                long bundleSize = 0;
                input.Position = 0;
                var reader = new BundleBinaryReader(input);
                while (reader.RemainingLength > 0)
                {
                    var pos = input.Position;
                    var signature = reader.ReadStringZeroTerm(4);
                    if (signature == PackSignature)
                    {
                        var isMr0k = reader.ReadBoolean();
                        var blockSize = BinaryPrimitives.ReadInt32LittleEndian(reader.ReadBytes(4));

                        var buffer = new byte[blockSize];
                        reader.Read(buffer);
                        if (isMr0k)
                        {
                            buffer = cipher.Decrypt(buffer);
                        }
                        ms.Write(buffer);

                        if (bundleSize == 0)
                        {
                            using var blockReader = new MemoryStream(buffer);
                            bundleSize = OtherBundleElementScanner.GetBundleFileSize(blockReader, 0);
                        }

                        readSize += buffer.Length;

                        if (readSize % (PackSize - 0x80) == 0)
                        {
                            var padding = PackSize - 9 - blockSize;
                            reader.Position += padding;
                        }

                        if (readSize == bundleSize)
                        {
                            readSize = 0;
                            bundleSize = 0;
                        }

                        continue;
                    }

                    reader.Position = pos;
                    signature = reader.ReadStringZeroTerm();
                    if (signature == FileStreamBundleHeader.UnityFSMagic)
                    {
                        var size = OtherBundleElementScanner.GetBundleFileSize(reader.BaseStream, pos);
                        reader.Position = pos;
                        reader.BaseStream.CopyTo(ms, size);
                        continue;
                    }

                    throw new InvalidOperationException($"Expected signature {PackSignature} or {FileStreamBundleHeader.UnityFSMagic}, got {signature} instead !!");
                }
            }
            finally
            {
                input.Dispose();
            }
            ms.Position = 0;
            return ms;
        }

        private Stream DecryptMark(Stream input)
        {
            var buffer = new byte[4];
            input.ReadExactly(buffer);
            if (Encoding.ASCII.GetString(buffer) != "mark")
            {
                input.Position = 0;
                return input;
            }

            const int BlockSize = 0xA00;
            const int ChunkSize = 0x264;
            const int ChunkPadding = 4;

            var blockPadding = ((BlockSize / ChunkSize) + 1) * ChunkPadding;
            var chunkSizeWithPadding = ChunkSize + ChunkPadding;
            var blockSizeWithPadding = BlockSize + blockPadding;

            var index = 0;
            var block = new byte[blockSizeWithPadding];
            var chunk = new byte[chunkSizeWithPadding];
            var output = new MemoryStream();
            while (input.Length != input.Position)
            {
                var readBlockBytes = input.Read(block);
                using var blockStream = new MemoryStream(block, 0, readBlockBytes);
                while (blockStream.Length != blockStream.Position)
                {
                    var readChunkBytes = blockStream.Read(chunk);
                    if (readBlockBytes == blockSizeWithPadding || readChunkBytes == chunkSizeWithPadding)
                    {
                        readChunkBytes -= ChunkPadding;
                    }
                    for (int i = 0; i < readChunkBytes; i++)
                    {
                        chunk[i] ^= CryptoKey.MarkKey[index++ % CryptoKey.MarkKey.Length];
                    }
                    output.Write(chunk, 0, readChunkBytes);
                }
            }

            input.Dispose();
            output.Position = 0;
            return output;
        }

    }
}
