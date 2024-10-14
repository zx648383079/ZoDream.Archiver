using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.BundleFiles
{
    public class FileStreamBundleReader : IArchiveReader
    {
        public FileStreamBundleReader(Stream stream, IArchiveOptions? options) : this(new EndianReader(stream, EndianType.BigEndian), options)
        {

        }

        public FileStreamBundleReader(EndianReader reader, IArchiveOptions? options)
        {
            _reader = reader;
            _options = options;
            _basePosition = reader.BaseStream.Position;
            _header.Read(reader);
            _headerLength = reader.BaseStream.Position - _basePosition;
        }

        private readonly EndianReader _reader;
        private readonly IArchiveOptions? _options;
        private readonly long _basePosition;
        private readonly long _headerLength;
        private readonly FileStreamBundleHeader _header = new();
        private SplitStreamCollection _storageItems = [];
        private long _dataBeginPosition;

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            if (entry is not FileStreamEntry ery)
            {
                return;
            }
            _reader.BaseStream.Position = _dataBeginPosition;
            using var ms = _storageItems.Create(_reader.BaseStream,
                ery.Offset, ery.Length);
            ms.CopyTo(output);
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            var entries = ReadEntry();
            var i = 0;
            foreach (var item in entries)
            {
                using var fs = File.Create(Path.Combine(folder, item.Name));
                ExtractTo(item, fs);
                progressFn?.Invoke((double)(++i) / entries.Count());
            }
        }

        public void Dispose()
        {
            if (_options?.LeaveStreamOpen == false)
            {
                _reader.Dispose();
            }
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            if (_header.Version >= BundleVersion.BF_LargeFilesSupport)
            {
                _reader.AlignStream(16);
            }
            if ((_header.Flags & BundleFlags.BlocksInfoAtTheEnd) != 0)
            {
                _reader.BaseStream.Seek(_basePosition + _header.Size - _header.CompressedBlocksInfoSize, SeekOrigin.Begin);
            }

            var metaCompression = (CompressionType)(_header.Flags & BundleFlags.CompressionTypeMask);
            using var reader = BundleCodec.Decode(_reader, metaCompression,
                _header.CompressedBlocksInfoSize,
                _header.UncompressedBlocksInfoSize);
            var metadataPosition = reader.BaseStream.Position;
            var uncompressedDataHash = reader.ReadBytes(16);

            _storageItems = new SplitStreamCollection(reader.ReadArray(StorageEntry.Read));
            var items = Array.Empty<FileStreamEntry>();
            if ((_header.Flags & BundleFlags.BlocksAndDirectoryInfoCombined) != 0)
            {
                items = reader.ReadArray(FileStreamEntry.Read);
            }
            if (_header.UncompressedBlocksInfoSize > 0)
            {
                if (reader.BaseStream.Position - metadataPosition != _header.UncompressedBlocksInfoSize)
                {
                    throw new Exception($"Read {reader.BaseStream.Position - metadataPosition} but expected {_header.UncompressedBlocksInfoSize} while reading bundle metadata");
                }
            }

            if ((_header.Flags & BundleFlags.BlocksInfoAtTheEnd) != 0)
            {
                _reader.BaseStream.Seek(_basePosition + _headerLength, SeekOrigin.Begin);
                if (_header.Version >= BundleVersion.BF_LargeFilesSupport)
                {
                    _reader.AlignStream(16);
                }
            }
            if ((_header.Flags & BundleFlags.BlockInfoNeedPaddingAtStart) != 0)
            {
                _reader.AlignStream(16);
            }
            _dataBeginPosition = _reader.BaseStream.Position;
            return items;
        }


        private void ReadFileStreamData()
        {


            //using var blockReader = new BundleFileBlockReader(stream, BlocksInfo);
            //foreach (FileStreamNode entry in DirectoryInfo.Nodes)
            //{
            //    try
            //    {
            //        SmartStream entryStream = blockReader.ReadEntry(entry);
            //        AddResourceFile(new ResourceFile(entryStream, FilePath, entry.Path));
            //    }
            //    catch (Exception ex)
            //    {
            //        AddFailedFile(new FailedFile()
            //        {
            //            Name = entry.Path,
            //            FilePath = FilePath,
            //            StackTrace = ex.ToString(),
            //        });
            //    }
            //}
        }
    }
}
