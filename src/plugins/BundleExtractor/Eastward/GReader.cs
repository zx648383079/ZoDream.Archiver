using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            if (entry is not ArchiveEntry o)
            {
                return;
            }
            if (!o.IsEncrypted)
            {
                reader.BaseStream.Seek(o.Offset, SeekOrigin.Begin);
                reader.ReadAsStream(o.CompressedLength).CopyTo(output);
                return;
            }
            using var decompressor = new Decompressor();
            var uncompressedSize = (int)o.Length;
            var compressedSize = (int)o.CompressedLength;
            var buffer = ArrayPool<byte>.Shared.Rent(compressedSize);
            var target = ArrayPool<byte>.Shared.Rent(uncompressedSize);
            try
            {
                reader.Read(buffer, 0, compressedSize);
                decompressor.Unwrap(buffer, 0, Compressor.GetCompressBound(uncompressedSize), target, 0, uncompressedSize);
                output.Write(target, 0, uncompressedSize);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
                ArrayPool<byte>.Shared.Return(target);
            }
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            var entries = ReadEntry().ToArray();
            var i = 0;
            foreach (var item in entries)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                var fileName = Path.Combine(folder, item.Name);
                if (!LocationStorage.TryCreate(fileName, mode, out fileName))
                {
                    return;
                }
                using var output = File.Create(fileName);
                ExtractTo(item, output);
                progressFn?.Invoke((double)(++i) / entries.Length);
            }
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
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
