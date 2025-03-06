using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;

namespace ZoDream.Shared.Bundle
{
    public class BundleChunk : IBundleChunk
    {
        public BundleChunk(string fileName)
            : this(fileName, "*.*")
        {
            
        }

        public BundleChunk(string fileName, string globPattern)
        {
            if (!string.IsNullOrWhiteSpace(globPattern))
            {
                _globPattern = globPattern;
            }
            if (!File.Exists(fileName))
            {
                Root = fileName;
                _countLazy = new Lazy<int>(() => BundleSource.FileCount([Root], globPattern));
                return;
            }
            Root = Path.GetDirectoryName(fileName)!;
            _fileItems = [fileName];
            _countLazy = new Lazy<int>(_fileItems.Count);
        }

        public BundleChunk(string baseFolder, IEnumerable<string> items)
        {
            Root = baseFolder;
            _fileItems = items;
            _countLazy = new Lazy<int>(_fileItems.Count);
        }

        private readonly IEnumerable<string>? _fileItems;
        private readonly string _globPattern = "*.*";
        public string Root { get; private set; }

        private readonly Lazy<int> _countLazy;
        public int Count => _countLazy.Value;

        public int Index { get; private set; }

        public string Create(string sourcePath, string outputFolder)
        {
            if (sourcePath.StartsWith(Root))
            {
                return Path.Combine(outputFolder, Path.GetRelativePath(Root, sourcePath));
            }
            if (sourcePath.StartsWith(outputFolder))
            {
                return sourcePath;
            }
            return Path.Combine(outputFolder, Path.GetFileName(sourcePath)); ;
        }

        public IEnumerator<string> GetEnumerator()
        {
            Index = 0;
            if (_fileItems is not null)
            {
                foreach (var item in _fileItems)
                {
                    Index++;
                    yield return item;
                }
                yield break;
            }
            var options = new EnumerationOptions()
            {
                RecurseSubdirectories = true,
                MatchType = MatchType.Win32,
                AttributesToSkip = FileAttributes.None,
                IgnoreInaccessible = false
            };
            var res = new FileSystemEnumerable<string>(Root, delegate (ref FileSystemEntry entry)
            {
                return entry.ToSpecifiedFullPath();
            }, options)
            {
                ShouldIncludePredicate = delegate (ref FileSystemEntry entry)
                {
                    if (entry.IsDirectory)
                    {
                        return false;
                    }
                    return BundleSource.IsMatch(entry.FileName, _globPattern);
                }
            };
            foreach (var item in res)
            {
                Index++;
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
