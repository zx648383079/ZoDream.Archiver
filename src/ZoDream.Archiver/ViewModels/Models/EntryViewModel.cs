using System;
using System.IO;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class EntryViewModel: BindableBase, ISourceEntry
    {
        private string _name = string.Empty;

        public string Name {
            get => _name;
            set => Set(ref _name, value);
        }


        public long Length { get; set; }

        public long CompressedLength { get; set; }

        public bool IsEncrypted { get; set; }
        public bool IsDirectory { get; set; }

        public DateTime? CreatedTime { get; set; }

        private string _fullPath = string.Empty;

        public string FullPath {
            get => _fullPath;
            set => Set(ref _fullPath, value);
        }

        public EntryViewModel(string fullPath)
        {
            FullPath = fullPath;
            if (Directory.Exists(fullPath))
            {
                var folder = new DirectoryInfo(fullPath);
                Name = folder.Name;
                IsDirectory = true;
                CreatedTime = folder.CreationTime;
                return;
            }
            var info = new FileInfo(fullPath);
            Name = info.Name;
            Length = info.Length;
            CreatedTime = info.CreationTime;
        }
        public EntryViewModel(string name, string fullPath)
            : this(fullPath)
        {
            Name = name;
        }

        public EntryViewModel(IReadOnlyEntry entry)
        {
            Name = entry.Name;
            Length = entry.Length;
            CompressedLength = entry.CompressedLength;
            IsEncrypted = entry.IsEncrypted;
            CreatedTime = entry.CreatedTime;
            if (entry is ISourceEntry e)
            {
                IsDirectory = e.IsDirectory;
                FullPath = e.FullPath;
            }
        }
    }
}
