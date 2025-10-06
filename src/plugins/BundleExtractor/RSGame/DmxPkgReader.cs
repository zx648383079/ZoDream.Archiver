using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.RSGame
{
    public class DmxPkgReader(Stream input) : IBundleHandler
    {
 
        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
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

        public void Dispose()
        {
            input.Dispose();
        }
    }
}
