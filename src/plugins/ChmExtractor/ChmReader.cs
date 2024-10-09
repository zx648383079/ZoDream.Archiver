using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.ChmExtractor
{
    public class ChmReader(BinaryReader reader, IArchiveOptions options) : IArchiveReader
    {
        
        private readonly long _basePosition = reader.BaseStream.Position;

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            if (entry is not ArchiveEntry item)
            {
                return;
            }
            LzxCodec.Decode(new PartialStream(reader.BaseStream, item.Offset, item.Length), output);
        }

        private void ExtractTo(IReadOnlyEntry entry, string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, mode, out var fullPath))
            {
                return;
            }
            using var fs = File.Create(fullPath);
            ExtractTo(entry, fs);
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            var entries = ReadEntry();
            var i = 0;
            foreach (var item in entries)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                var fileName = Path.Combine(folder, item.Name);
                ExtractTo(item, fileName, mode);
                progressFn?.Invoke((double)(++i) / entries.Count());
            }
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            reader.BaseStream.Seek(_basePosition, SeekOrigin.Begin);
            ReadHeader();
            ReadHeaderSectionTableEntry();
            return ReadHeaderSectionTableEntry().Where(i => i.Length > 0);

            //var pos = reader.ReadUInt64();
            //reader.BaseStream.Seek((long)pos, SeekOrigin.Begin);
            //ReadNameListFile();
            //ReadSectionData();
        }


        private void ReadHeader()
        {
            var magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            Debug.Assert(magic == "ITSF");
            var version = reader.ReadUInt32();
            var headerSize = reader.ReadUInt32();
            reader.ReadUInt32();
            var timestamp = BinaryPrimitives.ReadUInt32BigEndian(reader.ReadBytes(4));
            var languageId = (WindowsLanguageId)reader.ReadUInt32();
            var guids = reader.ReadBytes(16 * 2);
        }

        private ArchiveEntry[] ReadHeaderSectionTableEntry()
        {
            var offset = reader.ReadUInt64();
            var size = reader.ReadUInt64();
            var pos = reader.BaseStream.Position;
            reader.BaseStream.Seek((long)offset, SeekOrigin.Begin);
            var res = ReadHeaderSection();
            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
            return res;
        }

        private ArchiveEntry[] ReadHeaderSection()
        {
            var magic = reader.ReadBytes(4);
            if (magic.SequenceEqual((byte[])[0xFE, 0x01, 0x00, 0x00]))
            {
                reader.ReadUInt32();
                // 总文件尺寸
                var fileSize = reader.ReadUInt32();
                reader.ReadUInt32();
                reader.ReadUInt32();
                return [];
            } else if (Encoding.ASCII.GetString(magic) == "ITSP")
            {
                var version = reader.ReadUInt32();
                var directoryHeaderLength1 = reader.ReadUInt32();
                reader.ReadUInt32();
                var directoryChunkSize = reader.ReadUInt32();
                var quickRefSectionDensity = reader.ReadUInt32();
                var indexTreeDepth = reader.ReadUInt32();
                var rootIndexChunkNumber = reader.ReadUInt32();
                var firstPMGLChunkNumber = reader.ReadUInt32();
                var lastPMGLChunkNumber = reader.ReadUInt32();
                reader.ReadUInt32();
                var directoryChunkCount = reader.ReadUInt32();
                var languageId = (WindowsLanguageId)reader.ReadUInt32();
                var guid = reader.ReadBytes(16);
                var directoryHeaderLength2 = reader.ReadUInt32();
                reader.ReadUInt32();
                reader.ReadUInt32();
                reader.ReadUInt32();
                var res = new List<ArchiveEntry>();
                for (var i = 0; i < directoryChunkCount; i++)
                {
                    // Debug.WriteLine($"{i}: 0x{reader.BaseStream.Position:X}");
                    res.AddRange(ReadListingChunk(directoryChunkSize));
                }
                return [.. res];
            } else
            {
                throw new Exception("error");
            }
        }

        private ArchiveEntry ReadDirectoryListingEntry()
        {
            var nameLength = reader.Read7BitEncodedInt();
            var name = Encoding.ASCII.GetString(reader.ReadBytes(nameLength));
            var contentSection = reader.Read7BitEncodedInt();
            var offset = reader.Read7BitEncodedInt();
            var length = reader.Read7BitEncodedInt();

            return new ArchiveEntry(name, offset, length);
        }

        private void ReadDirectoryIndexEntry()
        {
            var nameLength = reader.Read7BitEncodedInt();
            var name = Encoding.ASCII.GetString(reader.ReadBytes(nameLength));
            var directoryListingChunk = reader.Read7BitEncodedInt();
        }

        private ArchiveEntry[] ReadListingChunk(uint directoryChunkSize)
        {
            var entryPos = reader.BaseStream.Position;
            var magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (magic == "PMGL")
            {
                var freeSpaceLength = reader.ReadUInt32();
                reader.ReadUInt32();
                var prevChunkNumber = reader.ReadUInt32();
                var nextChunkNumber = reader.ReadUInt32();
                var pos = reader.BaseStream.Position;
                entryPos += directoryChunkSize - 2;
                reader.BaseStream.Seek(entryPos, SeekOrigin.Begin);
                var directoryListingEntryCount = reader.ReadUInt16();
                reader.BaseStream.Seek(entryPos - (freeSpaceLength - 2), SeekOrigin.Begin);
                for (var i = 0; i < (freeSpaceLength - 2) / 2; i++)
                {
                    var offsets = reader.ReadUInt16();
                }
                reader.BaseStream.Seek(pos, SeekOrigin.Begin);
                var res = new ArchiveEntry[directoryListingEntryCount];
                for (var i = 0; i < directoryListingEntryCount; i++)
                {
                    res[i] = ReadDirectoryListingEntry();
                }
                reader.BaseStream.Seek(entryPos + 2, SeekOrigin.Begin);
                return res;
            } else if (magic == "PMGI")
            {
                var freeSpaceLength = reader.ReadUInt32();
                var pos = reader.BaseStream.Position;

                entryPos += directoryChunkSize - 2;
                reader.BaseStream.Seek(entryPos, SeekOrigin.Begin);
                var directoryIndexEntryCount = reader.ReadUInt16();
                reader.BaseStream.Seek(entryPos - (freeSpaceLength - 2), SeekOrigin.Begin);
                for (var i = 0; i < (freeSpaceLength - 2) / 2; i++)
                {
                    var offsets = reader.ReadUInt16();
                }

                reader.BaseStream.Seek(pos, SeekOrigin.Begin);
                var res = new ArchiveEntry[directoryIndexEntryCount];
                for (var i = 0; i < directoryIndexEntryCount; i++)
                {
                    res[i] = ReadDirectoryListingEntry();
                }
                reader.BaseStream.Seek(entryPos + 2, SeekOrigin.Begin);
                return res;
            } else
            {
                throw new Exception("error");
            }
        }

        private void ReadNameListEntry()
        {
            var nameLength = reader.ReadUInt16();
            var name = Encoding.Unicode.GetString(reader.ReadBytes(nameLength));

        }

        private void ReadNameListFile()
        {
            var fileLengthWords = reader.ReadUInt16();
            var entriesInFile = reader.ReadUInt16();
            for (var i = 0; i < entriesInFile; i++)
            {
                ReadNameListEntry();
            }
        }

        private void ReadSectionData()
        {
            var fileLengthWords = reader.ReadUInt32();
            var magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            Debug.Assert(magic == "LZXC");

            var version = reader.ReadUInt32();
            var lzxResetInterval = reader.ReadUInt32();
            var windowSize = reader.ReadUInt32();
            var cacheSize = reader.ReadUInt32();
            reader.ReadUInt32();
        }

        public void Dispose()
        {
            if (options?.LeaveStreamOpen == false)
            {
                reader.BaseStream.Dispose();
            }
        }
    }
}
