using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using ZoDream.Shared;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;
using ZstdSharp;

namespace ZoDream.BundleExtractor.Eastward
{
    public class GReader(EndianReader reader) : IArchiveReader
    {
        private const int MagicHeader = 27191;

        private readonly Dictionary<string, ArchiveEntry> _entries = ReadAll(reader).ToDictionary(i => i.Name);
        public IEnumerable<string> Keys => _entries.Keys;


        public Stream ReadAsStream(string name)
        {
            if (!_entries.TryGetValue(name, out var entry))
            {
                return EmptyStream.Null;
            }
            reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
            if (!entry.IsEncrypted)
            {
                return reader.ReadAsStream(entry.CompressedLength);
            }
            using var decompressor = new Decompressor();
            var uncompressedSize = (int)entry.Length;
            var compressedSize = (int)entry.CompressedLength;
            var buffer = ArrayPool<byte>.Shared.Rent(compressedSize);
            var target = new MemoryStream(uncompressedSize);
            try
            {
                reader.Read(buffer, 0, compressedSize);
                decompressor.Unwrap(buffer, 0, compressedSize, target.GetBuffer(), 0, uncompressedSize);
                return target;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }


        public void ReadDocument(string name, Action<JsonDocument> cb)
        {
            ExtractTo(name, fs => {
                using var doc = JsonDocument.Parse(fs);
                cb.Invoke(doc);
            });
        }

        public void ExtractTo(string name, Action<Stream> cb)
        {
            if (!_entries.TryGetValue(name, out var entry))
            {
                return;
            }
            ExtractTo(entry, cb);
        }
        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            if (entry is not ArchiveEntry o)
            {
                return;
            }
            ExtractTo(o, fs => fs.CopyTo(output));
        }
        
        private void ExtractTo(ArchiveEntry entry, Action<Stream> cb)
        {
            reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
            if (!entry.IsEncrypted)
            {
                cb.Invoke(reader.ReadAsStream(entry.CompressedLength));
                return;
            }
            using var decompressor = new Decompressor();
            var uncompressedSize = (int)entry.Length;
            var compressedSize = (int)entry.CompressedLength;
            var buffer = ArrayPool<byte>.Shared.Rent(compressedSize);
            var target = ArrayPool<byte>.Shared.Rent(uncompressedSize);
            try
            {
                reader.Read(buffer, 0, compressedSize);
                decompressor.Unwrap(buffer, 0, compressedSize, target, 0, uncompressedSize);
                using var ms = new MemoryStream(target, 0, uncompressedSize);
                cb.Invoke(ms);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
                ArrayPool<byte>.Shared.Return(target);
            }
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            var i = 0;
            foreach (var item in _entries)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                var fileName = Path.Combine(folder, item.Key);
                var extension = Path.GetExtension(item.Key);
                ExtractTo(item.Value, fs => {
                    if (extension == ".hmg")
                    {
                        if (LocationStorage.TryCreate(fileName, ".png", mode, out fileName))
                        {
                            using var output = File.Create(fileName);
                            new HmgReader(new BinaryReader(fs), item.Key).ExtractTo(output);
                        }
                        return;
                    }
                    if (LocationStorage.TryCreate(fileName, extension, mode, out fileName))
                    {
                        fs.SaveAs(fileName);
                    }
                });
                progressFn?.Invoke((double)(++i) / _entries.Count);
            }
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            return _entries.Values;
        }

        private static IEnumerable<ArchiveEntry> ReadAll(EndianReader reader)
        {
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            Expectation.ThrowIfNotSignature(reader.ReadInt32() == MagicHeader);
            var length = reader.ReadInt32();
            for (int i = 0; i < length; i++)
            {
                var name = reader.ReadStringZeroTerm();
                var offset = reader.ReadInt32();
                var isCompressed = reader.ReadInt32() == 2;
                var decompressedSize = reader.ReadInt32();
                var compressedSize = reader.ReadInt32();
                var pos = reader.BaseStream.Position;
                yield return new ArchiveEntry(name, offset, decompressedSize, compressedSize, isCompressed, null);
                reader.BaseStream.Position = pos;
            }
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        internal static bool IsSupport(ReadOnlySpan<byte> buffer)
        {
            return BitConverter.ToInt32(buffer) == MagicHeader;
        }
    }
}
