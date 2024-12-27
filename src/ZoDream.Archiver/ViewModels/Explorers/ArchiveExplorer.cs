using System.Collections.Generic;
using System.Linq;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Archiver.ViewModels
{
    public class ArchiveExplorer(IArchiveReader reader, IEntryService service) : IEntryExplorer
    {
        public List<IReadOnlyEntry> Items { get; private set; } = reader.ReadEntry().ToList();
        public bool CanGoBack { get; }

        public IEntryStream Open(ISourceEntry entry)
        {
            if (entry.IsDirectory)
            {
                return OpenDirectory(entry.FullPath);
            }
            return new UnknownEntryStream();
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
                }
                yield return new DirectoryEntry(childPath);
            }
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
            var j = fileName.IndexOf('/', i);
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
