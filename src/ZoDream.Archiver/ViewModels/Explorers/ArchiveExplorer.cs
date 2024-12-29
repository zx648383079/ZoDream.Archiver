using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.Archiver.ViewModels
{
    public class ArchiveExplorer(IArchiveReader reader, IReadOnlyEntry[] entries, IEntryService service) : IEntryExplorer
    {

        public ArchiveExplorer(IArchiveReader reader, IEntryService service)
            : this (reader, reader.ReadEntry().ToArray(), service)
        {
            
        }

        public List<IReadOnlyEntry> Items { get; private set; } = entries.ToList();
        public bool CanGoBack { get; }

        public async Task<IEntryStream> OpenAsync(ISourceEntry entry)
        {
            if (entry.IsDirectory)
            {
                return OpenDirectory(entry.FullPath);
            }
            var mime = MimeMapping.MimeUtility.GetMimeMapping(entry.Name);
            if (mime.StartsWith("image/"))
            {
                return new ImageEntryStream(this, entry);
            } else if (mime.StartsWith("audio/")) 
            {
                return new AudioEntryStream(this, entry);
            }
            else if (mime.StartsWith("video/"))
            {
                return new VideoEntryStream(this, entry);
            }
            else if (mime.StartsWith("text/"))
            {
                return new TextEntryStream(this, entry);
            }
            return UnknownEntryStream.Instance;
        }

        public void SaveAs(ISourceEntry entry, Stream output)
        {
            if (TryGet(entry, out var file))
            {
                reader.ExtractTo(file, output);
            }
        }

        public void SaveAs(ISourceEntry entry, string folder,
           ArchiveExtractMode mode,
           CancellationToken token = default)
        {
            foreach (var item in Items)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                if ((entry.IsDirectory && !IsChildPath(entry.FullPath, item.Name)) || (!entry.IsDirectory && entry.FullPath != item.Name))
                {
                    continue;
                }
                var fullPath = entry.IsDirectory ?
                    Path.Combine(folder, entry.Name, item.Name[(entry.FullPath.Length + 1)..])
                    : Path.Combine(folder, entry.Name);
                if (!LocationStorage.TryCreate(fullPath, mode, out fullPath))
                {
                    continue;
                }
                using var fs = File.Create(fullPath);
                reader.ExtractTo(item, fs);
            }
        }

        private bool TryGet(ISourceEntry file, [NotNullWhen(true)] out IReadOnlyEntry? entry)
        {
            foreach (var item in Items)
            {
                if (item.Name == file.FullPath)
                {
                    entry = item;
                    return true;
                }
            }
            entry = null;
            return false;
        }

        private DirectoryEntryStream OpenDirectory(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return new(GetFiles(string.Empty).ToArray());
            }
            var last = fileName.LastIndexOf('/');
            var top = last < 0 ?
                new TopDirectoryEntry(string.Empty) :
                new TopDirectoryEntry(fileName[0..last]);
            return new([top, .. GetFiles(fileName)]);
        }

        private IEnumerable<ISourceEntry> GetFiles(string fileName)
        {
            var exist = new List<string>();
            foreach (var entry in Items)
            {
                var childPath = GetChildPath(fileName, entry.Name);
                if (string.IsNullOrEmpty(childPath) || exist.Contains(childPath))
                {
                    continue;
                }
                exist.Add(childPath);
                if (childPath == entry.Name)
                {
                    yield return new FileEntry(entry);
                } else
                {
                    yield return new DirectoryEntry(childPath);
                }
            }
        }

        private bool IsChildPath(string folder, string fileName)
        {
            if (!string.IsNullOrEmpty(folder) && !fileName.StartsWith(folder))
            {
                return false;
            }
            var i = folder.Length;
            if (i > 0 && fileName[i] != '/')
            {
                return false;
            }
            if (i == fileName.Length)
            {
                return false;
            }
            return true;
        }

        private string GetChildPath(string folder, string fileName)
        {
            if (!string.IsNullOrEmpty(folder) && !fileName.StartsWith(folder))
            {
                return string.Empty;
            }
            var i = folder.Length;
            if (i > 0 && fileName[i] != '/')
            {
                return string.Empty;
            }
            if (i == fileName.Length)
            {
                return string.Empty;
            }
            var j = fileName.IndexOf('/', i + 1);
            if (j < 0)
            {
                return fileName;
            }
            return fileName[..j];
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
