using System;
using System.IO;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class EntryViewModel: BindableBase, IReadOnlyEntry
    {
        private string _name = string.Empty;

        public string Name {
            get => _name;
            set => Set(ref _name, value);
        }


        public long Length { get; set; }

        public long CompressedLength { get; set; }

        public bool IsEncrypted { get; set; }

        public DateTime? CreatedTime { get; set; }

        private string _fullPath = string.Empty;

        public string FullPath {
            get => _fullPath;
            set => Set(ref _fullPath, value);
        }

        public EntryViewModel(string fullPath)
        {
            FullPath = fullPath;
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
    }
}
