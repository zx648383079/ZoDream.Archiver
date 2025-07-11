using System;
using System.Diagnostics;
using ZoDream.Shared.Interfaces;
using IOPath = System.IO.Path;

namespace ZoDream.Shared.Models
{
    public struct FilePathName(string name) : IFileName
    {
        public readonly string Name => name;

        public readonly override string ToString()
        {
            return Name;
        }

        public readonly bool Equals(IFileName? other)
        {
            return Name.Equals(other?.Name);
        }

        public readonly bool Equals(IFileName? other, StringComparison comparisonType)
        {
            return Name.Equals(other?.Name, comparisonType);
        }

        public readonly bool Equals(IFilePath? other)
        {
            return Name.Equals(other?.Name);
        }

        public readonly bool Equals(IFilePath? other, StringComparison comparisonType)
        {
            return Name.Equals(other?.Name, comparisonType);
        }

        public readonly bool Equals(IEntryPath? other)
        {
            return Name.Equals(other?.Name);
        }

        public readonly bool Equals(IEntryPath? other, StringComparison comparisonType)
        {
            return Name.Equals(other?.Name, comparisonType);
        }

        public readonly bool Equals(string? other)
        {
            return Name.Equals(other);
        }

        public readonly bool Equals(string? other, StringComparison comparisonType)
        {
            return Name.Equals(other, comparisonType);
        }
    }

    public struct FilePath(string path) : IFilePath
    {
        public readonly string FullPath => path;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public readonly string Name => IOPath.GetFileName(FullPath);

        public override readonly string ToString()
        {
            return FullPath;
        }

        public readonly bool Equals(IFilePath? other)
        {
            return Equals(other?.FullPath);
        }

        public readonly bool Equals(IFilePath? other, StringComparison comparisonType)
        {
            return Equals(other?.FullPath, comparisonType);
        }

        public readonly bool Equals(string? other)
        {
            return FullPath.Equals(other);
        }

        public readonly bool Equals(string? other, StringComparison comparisonType)
        {
            return FullPath.Equals(other, comparisonType);
        }

        public readonly bool Equals(IFileName? other)
        {
            if (other is IEntryName)
            {
                return false;
            }
            return Name.Equals(other?.Name);
        }

        public readonly bool Equals(IFileName? other, StringComparison comparisonType)
        {
            if (other is IEntryName)
            {
                return false;
            }
            return Name.Equals(other?.Name, comparisonType);
        }

        public readonly bool Equals(IEntryPath? other)
        {
            return false;
        }

        public static implicit operator FilePath(string path)
        {
            return new FilePath(path);
        }

        public static implicit operator string(FilePath path)
        {
            return path.FullPath;
        }

        public readonly IFilePath Combine(string name)
        {
            return new FileEntryPath(FullPath, name);
        }

        public readonly IFilePath Adjacent(string name)
        {
            var folder = IOPath.GetDirectoryName(FullPath);
            if (string.IsNullOrEmpty(folder))
            {
                return new FilePath(name);
            }
            return new FilePath(IOPath.Combine(folder, name));
        }

        public static IFilePath Parse(string path)
        {
            var i = path.LastIndexOf(FileEntryPath.Separator);
            if (i < 0)
            {
                return new FilePath(path);
            }
            var entryName = path[(i + FileEntryPath.Separator.Length)..];
            if (string.IsNullOrWhiteSpace(entryName))
            {
                return new FilePath(path[..i]);
            }
            return new FileEntryPath(path[..i], entryName);
        }

        
        /// <summary>
        /// 获取真实文件路径
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetFilePath(IFilePath source)
        {
            if (source is IEntryPath e)
            {
                return e.FilePath;
            }
            return source.FullPath;
        }
    }

    public struct EntryName(string name) : IEntryName
    {
        public readonly string EntryPath => name;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public readonly string Name => EntryPath;

        public readonly override string ToString()
        {
            return $"{FileEntryPath.Separator}{EntryPath}";
        }

        public readonly bool Equals(IEntryName? other, StringComparison comparisonType)
        {
            return EntryPath.Equals(other?.EntryPath, comparisonType);
        }

        public readonly bool Equals(IEntryName? other)
        {
            return EntryPath.Equals(other?.EntryPath);
        }
        public readonly bool Equals(IEntryPath? other, StringComparison comparisonType)
        {
            return EntryPath.Equals(other?.EntryPath, comparisonType);
        }
        public readonly bool Equals(IEntryPath? other)
        {
            return EntryPath.Equals(other?.EntryPath);
        }

        public readonly bool Equals(string? other)
        {
            return Equals(other, StringComparison.Ordinal);
        }

        public readonly bool Equals(string? other, StringComparison comparisonType)
        {
            if (string.IsNullOrEmpty(other))
            {
                return EntryPath.Equals(other);
            }
            var i = other.LastIndexOf(FileEntryPath.Separator);
            if (i >= 0)
            {
                return EntryPath.Equals(other[(i + FileEntryPath.Separator.Length)..], comparisonType);
            }
            return EntryPath.Equals(other, comparisonType);
        }

        public readonly bool Equals(IFileName? other)
        {
            if (other is IEntryName n)
            {
                return n.Name.Equals(Name);
            }
            return false;
        }

        public readonly bool Equals(IFilePath? other)
        {
            return false;
        }

        public readonly bool Equals(IFileName? other, StringComparison comparisonType)
        {
            if (other is IEntryName n)
            {
                return Equals(n, comparisonType);
            }
            return false;
        }

        public static implicit operator EntryName(string path)
        {
            return new EntryName(path);
        }

        public static implicit operator string(EntryName path)
        {
            return path.EntryPath;
        }
    }

    public struct FileEntryPath(string filePath, string entryName) : IEntryPath
    {
        public const string Separator = "#";
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public readonly string FilePath => filePath;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public readonly string EntryPath => entryName;

        public readonly string FullPath => string.IsNullOrWhiteSpace(EntryPath) ?
            FilePath : $"{FilePath}{Separator}{EntryPath}";

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public readonly string Name => IOPath.GetFileName(EntryPath);

        public readonly override string ToString()
        {
            return FullPath;
        }

        public readonly bool Equals(IFilePath? other)
        {
            if (other is IEntryPath e)
            {
                return Equals(e);
            }
            return false;
        }

        public readonly bool Equals(IFilePath? other, StringComparison comparisonType)
        {
            if (other is IEntryPath e)
            {
                return Equals(e, comparisonType);
            }
            return false;
        }

        public readonly bool Equals(string? other)
        {
            return FullPath.Equals(other);
        }

        public readonly bool Equals(string? other, StringComparison comparisonType)
        {
            return FullPath.Equals(other, comparisonType);
        }

        public readonly bool Equals(IEntryPath? other)
        {
            return FilePath.Equals(other?.FilePath) && EntryPath.Equals(other?.EntryPath);
        }

        public readonly bool Equals(IEntryPath? other, StringComparison comparisonType)
        {
            return FilePath.Equals(other?.FilePath, comparisonType) 
                && EntryPath.Equals(other?.EntryPath, comparisonType);
        }

        public readonly bool Equals(IEntryName? other)
        {
            if (other is IEntryPath e)
            {
                return Equals(e);
            }
            return EntryPath.Equals(other?.EntryPath);
        }

        public readonly bool Equals(IEntryName? other, StringComparison comparisonType)
        {
            if (other is IEntryPath e)
            {
                return Equals(e, comparisonType);
            }
            return EntryPath.Equals(other?.EntryPath, comparisonType);
        }

        public readonly bool Equals(IFileName? other)
        {
            if (other is IEntryName n)
            {
                return Equals(n);
            }
            return false;
        }

        public readonly bool Equals(IFileName? other, StringComparison comparisonType)
        {
            if (other is IEntryName n)
            {
                return Equals(n, comparisonType);
            }
            return false;
        }

        public readonly IFilePath Combine(string name)
        {
            return new FileEntryPath(FilePath,
                string.IsNullOrWhiteSpace(EntryPath) ? name : IOPath.Combine(EntryPath, name));
        }

        public IFilePath Adjacent(string name)
        {
            var folder = IOPath.GetDirectoryName(EntryPath);
            if (string.IsNullOrEmpty(folder))
            {
                return new FileEntryPath(FilePath, name);
            }
            return new FileEntryPath(FilePath, IOPath.Combine(folder, name));
        }

        public static implicit operator FileEntryPath(string path)
        {
            var i = path.LastIndexOf(Separator);
            if (i < 0)
            {
                return new FileEntryPath(path, string.Empty);
            }
            return new FileEntryPath(path[..i], path[(i + Separator.Length)..]);
        }

        public static implicit operator string(FileEntryPath path)
        {
            return path.FullPath;
        }
    }
}
