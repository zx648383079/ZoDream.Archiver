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
                _entranceItems = [fileName];
                return;
            }
            _entranceItems = [Path.GetDirectoryName(fileName)!];
            _fileItems = [fileName];
            _count = _fileItems.Count();
        }

        public BundleChunk(string baseFolder, IEnumerable<string> items)
        {
            _entranceItems = [baseFolder];
            _fileItems = items;
            _count = _fileItems.Count();
        }

        public BundleChunk(string[] entranceItems, IEnumerable<string> items)
        {
            _entranceItems = entranceItems;
            _fileItems = items;
            _count = _fileItems.Count();
        }

        private readonly IEnumerable<string>? _fileItems;
        private readonly string _globPattern = "*.*";
        /// <summary>
        /// 可能的入口文件
        /// </summary>
        private readonly string[] _entranceItems;

        private int _count = -1;
        public int Count 
        { 
            get {
                if (_count >= 0)
                {
                    return _count;
                }
                _count = 0;
                _count = (int)BundleStorage.FileCount(_entranceItems, _globPattern);
                return _count;
            }
        }

        public int Index { get; private set; }
        /// <summary>
        /// 根据入口获取路径
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        private string GetRelativePath(string fullPath)
        {
            foreach (var item in _entranceItems)
            {
                if (!fullPath.StartsWith(item))
                {
                    continue;
                }
                if (fullPath.Length == item.Length)
                {
                    break;
                }
                return Path.GetRelativePath(item, fullPath);
            }
            return Path.GetFileName(fullPath);
        }

        public bool IsExportable(string sourcePath)
        {
            foreach (var item in _entranceItems)
            {
                if (sourcePath.StartsWith(item))
                {
                    return true;
                }
            }
            return false;
        }

        public string Create(string sourcePath, string outputFolder)
        {
            if (sourcePath.StartsWith(outputFolder))
            {
                return sourcePath;
            }
            return Path.Combine(outputFolder, GetRelativePath(sourcePath));
        }

        public IBundleChunk Repack(IEnumerable<string> fileItems)
        {
            return new BundleChunk(_entranceItems, fileItems);
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
            var res = new FileSystemEnumerable<string>(_entranceItems[0], delegate (ref FileSystemEntry entry)
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
                    return BundleStorage.IsMatch(entry.FileName, _globPattern);
                }
            };
            foreach (var item in res)
            {
                Index++;
                yield return item;
            }
            if (Index > _count)
            {
                _count = Index;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
