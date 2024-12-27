using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZoDream.Shared.Compression;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;

namespace ZoDream.Archiver.ViewModels
{
    public class StorageExplorer(IEntryService service): IEntryExplorer
    {

        public List<ISourceEntry> Items { get; private set; } = [];

        public bool CanGoBack { get; }

        public bool Contains(string fileName)
        {
            return Items.FindIndex(i => i.FullPath == fileName) >= 0;
        }

        public void Add(IEnumerable<string> fileNames)
        {
            foreach (var item in fileNames)
            {
                Add(item);
            }
        }

        public void Add(IEnumerable<ISourceEntry> fileNames)
        {
            foreach (var item in fileNames)
            {
                Add(item.FullPath);
            }
        }

        public void Add(ISourceEntry entry)
        {
            Add(entry.FullPath);
        }

        public void Add(string fileName)
        {
            var fullPath = Path.GetFullPath(fileName);
            if (Contains(fullPath))
            {
                return;
            }
            if (Directory.Exists(fullPath))
            {
                Items.Add(new DirectoryEntry(fullPath));
            } else if (File.Exists(fullPath))
            {
                Items.Add(new FileEntry(fullPath));
            }
        }

        public void Remove(string fileName)
        {
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                if (Items[i].FullPath == fileName)
                {
                    Items.RemoveAt(i);
                }
            }
        }

        public IEntryStream Open(ISourceEntry entry)
        {
            if (entry.IsDirectory)
            {
                return OpenDirectory(entry.FullPath);
            }
            var input = File.OpenRead(entry.FullPath);
            // var input = new CachedStream(fs, 256);
            var reader = App.ViewModel.Plugin.TryGetReader(input, entry.FullPath, null, out _);
            if (reader is null)
            {
                return new UnknownEntryStream();
            }
            return new ArchiveEntryStream(new ArchiveExplorer(reader, service));
        }

        private DirectoryEntryStream OpenDirectory(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return new([.. Items]);
            }
            var info = new DirectoryInfo(fileName);
            var top = Contains(fileName) ?
                new TopDirectoryEntry(string.Empty) : 
                new TopDirectoryEntry(info.Parent!.FullName);
            return new([top, 
                .. info.GetDirectories().Select(i => new DirectoryEntry(i)).ToArray(),
                .. info.GetFiles().Select(i => new FileEntry(i)).ToArray()]);
        }

        public void Dispose()
        {
            Items.Clear();
        }

        
    }
}
