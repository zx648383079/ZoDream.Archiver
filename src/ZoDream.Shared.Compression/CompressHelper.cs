using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Compression
{
    public static class CompressHelper
    {
        public static ReaderOptions? Convert(IArchiveOptions? options)
        {
            if (options is null)
            {
                return null;
            }
            return new ReaderOptions()
            {
                LeaveStreamOpen = options.LeaveStreamOpen,
                LookForHeader = options.LookForHeader,
                Password = options.Password,
            };
        }

        public static IReadOnlyEntry Convert(IEntry item)
        {
            return new ReadOnlyEntry(item.Key ?? string.Empty, item.Size, item.CompressedSize, item.IsEncrypted, item.LastModifiedTime);
        }

        internal static void ExtractToDirectory(IArchive archive, 
            string outputFolder, 
            ArchiveExtractMode mode, Action<double>? progressFn = null, 
            CancellationToken token = default)
        {
            var options = new ExtractionOptions()
            {
                Overwrite = mode == ArchiveExtractMode.Overwrite,
            };
            if (archive.IsSolid || archive.Type == ArchiveType.SevenZip)
            {
                using var reader = archive.ExtractAllEntries();
                reader.WriteAllToDirectory(outputFolder, options);
                return;
            }
            var totalBytes = archive.TotalUncompressSize;
            var bytesRead = 0L;

            var seenDirectories = new HashSet<string>();

            foreach (var entry in archive.Entries)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                if (entry.IsDirectory)
                {
                    var dirPath = Path.Combine(
                        outputFolder,
                        entry.Key!
                    );
                    if (
                        Path.GetDirectoryName(dirPath + "/") is { } parentDirectory
                        && seenDirectories.Add(dirPath)
                    )
                    {
                        Directory.CreateDirectory(parentDirectory);
                    }
                    continue;
                }
                entry.WriteToDirectory(outputFolder, options);

                bytesRead += entry.Size;
                progressFn?.Invoke(bytesRead / totalBytes);
            }
        }
    }
}
