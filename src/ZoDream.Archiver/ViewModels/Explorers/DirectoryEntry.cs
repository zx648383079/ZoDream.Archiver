using System;
using System.IO;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Archiver.ViewModels
{
    public class DirectoryEntry : ISourceEntry
    {
        public static readonly DirectoryEntry Empty = new(string.Empty);
        public bool IsDirectory => true;

        public string FullPath { get; private set; }

        public string Name { get; private set; }

        public long Length => 0;

        public long CompressedLength => 0;

        public bool IsEncrypted => false;

        public DateTime? CreatedTime => null;

        public DirectoryEntry(string fileName)
        {
            FullPath = fileName;
            Name = Path.GetFileName(fileName);
        }

        public DirectoryEntry(DirectoryInfo info)
        {
            FullPath = info.FullName;
            Name = info.Name;
        }
    }

    public class TopDirectoryEntry(string fileName) : DirectoryEntry(fileName)
    {
    }

    public class FileEntry : ReadOnlyEntry, ISourceEntry
    {
        public bool IsDirectory => false;
        public string FullPath { get; private set; } = string.Empty;

        public FileEntry(string fileName)
            : this(new FileInfo(fileName))
        {
        }

        public FileEntry(IReadOnlyEntry entry)
            : base(Path.GetFileName(entry.Name), entry.CompressedLength, entry.Length, entry.IsEncrypted, entry.CreatedTime)
        {
            if (entry is ISourceEntry e)
            {
                FullPath = e.FullPath;
            } else
            {
                FullPath = entry.Name;
            }
        }

        public FileEntry(FileInfo file)
            : base(file.Name, file.Length, file.Length, false, file.CreationTime)
        {
            FullPath = file.FullName;
        }
    }
}
