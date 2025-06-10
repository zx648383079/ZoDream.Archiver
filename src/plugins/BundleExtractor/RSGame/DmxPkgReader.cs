using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.RSGame
{
    public class DmxPkgReader(Stream input) : IArchiveReader
    {
 

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            throw new NotImplementedException();
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            using var archive = ZipArchive.Open(input);
            var dicItems = new Dictionary<string, ZipArchiveEntry>();
            var datItems = new Dictionary<string, ZipArchiveEntry>();
            foreach (var item in archive.Entries)
            {
                if (item.IsDirectory || string.IsNullOrEmpty(item.Key))
                {
                    continue;
                }
                if (item.Key.EndsWith(".dic"))
                {
                    dicItems.Add(item.Key[..^4], item);
                }
                else if (item.Key.EndsWith(".dat"))
                {
                    datItems.Add(item.Key[..^4], item);
                }
            }
            foreach (var item in dicItems)
            {
                if (!datItems.TryGetValue(item.Key, out var dat))
                {
                    continue;
                }
                using var dic = new BinaryReader(item.Value.OpenEntryStream());
                using var fs = dat.OpenEntryStream();
                var count = dic.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    var nameLength = dic.ReadByte();
                    var buffer = dic.ReadBytes(nameLength).Select(j => (byte)(j ^ nameLength)).ToArray();
                    var name = Encoding.ASCII.GetString(buffer);
                    var offset = dic.ReadUInt32() - buffer[^6] - nameLength;
                    var size = dic.ReadUInt32() - buffer[^7] - nameLength;
                    if (!LocationStorage.TryCreate(Path.Combine(folder, name),
                        mode,
                        out var fileName
                        ))
                    {
                        continue;
                    }
                    new PartialStream(fs, offset, size).SaveAs(fileName);
                }
            }
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            using var archive = ZipArchive.Open(input);
            foreach (var item in archive.Entries)
            {
                if (item.IsDirectory || string.IsNullOrEmpty(item.Key))
                {
                    continue;
                }
                if (!item.Key.EndsWith(".dic"))
                {
                    continue;
                }
                using var dic = new BinaryReader(item.OpenEntryStream());
                var count = dic.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    var nameLength = dic.ReadByte();
                    var buffer = dic.ReadBytes(nameLength).Select(j => (byte)(j ^ nameLength)).ToArray();
                    yield return new ArchiveEntry(Encoding.ASCII.GetString(buffer), 
                        dic.ReadUInt32() - buffer[^6] - nameLength, 
                        dic.ReadUInt32() - buffer[^7] - nameLength);
                }
            }
        }

        public void Dispose()
        {
            input.Dispose();
        }
    }
}
