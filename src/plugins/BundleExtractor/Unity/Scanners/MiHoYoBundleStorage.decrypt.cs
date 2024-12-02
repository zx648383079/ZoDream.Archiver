using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class MiHoYoElementScanner
    {

        private Stream DecryptPack(Stream input)
        {
            const int PackSize = 0x880;
            const string PackSignature = "pack";
            const string UnityFSSignature = "UnityFS";

            var data = new byte[input.Length];
            input.ReadExactly(data);
            var packIdx = data.Search(PackSignature);
            if (packIdx == -1)
            {
                input.Position = 0;
                return input;
            }
            Logger.Verbose($"Found signature {PackSignature} at offset 0x{packIdx:X8}");
            var mr0kIdx = data.Search("mr0k", packIdx);
            if (mr0kIdx == -1)
            {
                input.Position = 0;
                return input;
            }
            var ms = new MemoryStream();
            try
            {
                var mr0k = (Mr0k)game;

                long readSize = 0;
                long bundleSize = 0;
                reader.Position = 0;
                while (reader.Remaining > 0)
                {
                    var pos = reader.Position;
                    var signature = reader.ReadStringToNull(4);
                    if (signature == PackSignature)
                    {
                        Logger.Verbose($"Found {PackSignature} chunk at position {reader.Position - PackSignature.Length}");
                        var isMr0k = reader.ReadBoolean();
                        Logger.Verbose("Chunk is mr0k encrypted");
                        var blockSize = BinaryPrimitives.ReadInt32LittleEndian(reader.ReadBytes(4));

                        Logger.Verbose($"Chunk size is 0x{blockSize:X8}");
                        Span<byte> buffer = new byte[blockSize];
                        reader.Read(buffer);
                        if (isMr0k)
                        {
                            buffer = Mr0kUtils.Decrypt(buffer, mr0k);
                        }
                        ms.Write(buffer);

                        if (bundleSize == 0)
                        {
                            Logger.Verbose("This is header chunk !! attempting to read the bundle size");
                            using var blockReader = new EndianBinaryReader(new MemoryStream(buffer.ToArray()));
                            var header = new Header()
                            {
                                signature = blockReader.ReadStringToNull(),
                                version = blockReader.ReadUInt32(),
                                unityVersion = blockReader.ReadStringToNull(),
                                unityRevision = blockReader.ReadStringToNull(),
                                size = blockReader.ReadInt64()
                            };
                            bundleSize = header.size;
                            Logger.Verbose($"Bundle size is 0x{bundleSize:X8}");
                        }

                        readSize += buffer.Length;

                        if (readSize % (PackSize - 0x80) == 0)
                        {
                            var padding = PackSize - 9 - blockSize;
                            reader.Position += padding;
                            Logger.Verbose($"Skip 0x{padding:X8} padding");
                        }

                        if (readSize == bundleSize)
                        {
                            Logger.Verbose($"Bundle has been read entirely !!");
                            readSize = 0;
                            bundleSize = 0;
                        }

                        continue;
                    }

                    reader.Position = pos;
                    signature = reader.ReadStringToNull();
                    if (signature == UnityFSSignature)
                    {
                        Logger.Verbose($"Found {UnityFSSignature} chunk at position {reader.Position - (UnityFSSignature.Length + 1)}");
                        var header = new Header()
                        {
                            signature = reader.ReadStringToNull(),
                            version = reader.ReadUInt32(),
                            unityVersion = reader.ReadStringToNull(),
                            unityRevision = reader.ReadStringToNull(),
                            size = reader.ReadInt64()
                        };

                        Logger.Verbose($"Bundle size is 0x{header.size:X8}");
                        reader.Position = pos;
                        reader.BaseStream.CopyTo(ms, header.size);
                        continue;
                    }

                    throw new InvalidOperationException($"Expected signature {PackSignature} or {UnityFSSignature}, got {signature} instead !!");
                }
            }
            catch (InvalidCastException)
            {
                Logger.Error($"Game type mismatch, Expected {nameof(GameType.GI_Pack)} ({nameof(Mr0k)}) but got {game.Name} ({game.GetType().Name}) !!");
            }
            catch (Exception e)
            {
                Logger.Error($"Error while reading pack file {reader.FullPath}", e);
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
