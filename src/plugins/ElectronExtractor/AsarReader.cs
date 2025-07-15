using System;
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

namespace ZoDream.ElectronExtractor
{
    public class AsarReader(EndianReader reader, IArchiveOptions options) : IArchiveReader
    {
        internal static readonly byte[] Signature = [0x04, 0x00, 0x00, 0x00];

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            if (entry is not ArchiveEntry e)
            {
                return;
            }
            new PartialStream(reader.BaseStream, e.Offset, e.Length).CopyTo(output);
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
            var items = ReadEntry().ToArray();
            var i = 0;
            foreach (var item in items)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                var fileName = Path.Combine(folder, Path.GetFileName(item.Name.Replace('/', '\\')));
                ExtractTo(item, fileName, mode);
                progressFn?.Invoke((double) (++ i) / items.Length);
            }
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            var signature = reader.ReadBytes(4);
            Expectation.ThrowIfNotSignature(signature.SequenceEqual(Signature));
            var headerSize = reader.ReadUInt32();
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            var headerMetadataSize = reader.ReadUInt32();
            var header = reader.ReadString((int)headerMetadataSize);
            var filesOffset = headerSize + 8;
            using var doc = JsonDocument.Parse(header);
            if (doc == null)
            {
                yield break;
            }
            var root = doc.RootElement;
            if (!root.TryGetProperty("files", out var element))
            {
                yield break;
            }
            foreach (var item in ReadDictionary(element, string.Empty))
            {
                item.Move(filesOffset);
                yield return item;
            }
        }

        private IEnumerable<ArchiveEntry> ReadDictionary(JsonElement element, string folder)
        {
            foreach (var item in element.EnumerateObject())
            {
                if (item.Value.TryGetProperty("files", out var ele))
                {
                    foreach (var it in ReadDictionary(ele, $"{folder}{item.Name}/"))
                    {
                        yield return it;
                    }
                    continue;
                }
                yield return new ArchiveEntry($"{folder}{item.Name}", ReadInt64(item.Value, "offset"), ReadInt64(item.Value, "size"));
            }
        }

        private static int ReadInt(JsonElement element, string key)
        {
            if (element.TryGetProperty(key, out var res))
            {
                return res.GetInt32();
            }
            return 0;
        }

        private static long ReadInt64(JsonElement element, string key)
        {
            if (!element.TryGetProperty(key, out var res))
            {
                return 0;
            }
            if (res.ValueKind == JsonValueKind.Number && res.TryGetInt64(out var r))
            {
                return r;
            }
            if (long.TryParse(res.GetString(), out r))
            {
                return r;
            }
            return 0;
        }

        public void Dispose()
        {
            if (options?.LeaveStreamOpen == false)
            {
                reader.Dispose();
            }
        }
    }
}
