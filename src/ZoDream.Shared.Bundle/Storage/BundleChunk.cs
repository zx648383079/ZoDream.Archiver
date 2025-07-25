﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

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

        public BundleChunk(string baseFolder, string[] items)
        {
            _entranceItems = [baseFolder];
            _fileItems = items;
            _count = _fileItems.Count();
        }

        public BundleChunk(string[] entranceItems, string[] items, int effectiveCount)
        {
            _entranceItems = entranceItems;
            _fileItems = items;
            _count = _fileItems.Length;
            _effectiveCount = effectiveCount;
        }

        private readonly string[]? _fileItems;
        private readonly string _globPattern = "*.*";
        /// <summary>
        /// 可能的入口文件
        /// </summary>
        private readonly string[] _entranceItems;
        private readonly int _effectiveCount;

        public IBundleFilter? Filter { get; set; }

        public IBundleMapper? Mapper { get; set; }

        public int EffectiveCount => _effectiveCount > 0 ? _effectiveCount : Index;

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
            if (Filter is not null)
            {
                return !Filter.IsExclude(sourcePath);
            }
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
            if (Mapper?.TryGet(sourcePath, out var toPath) == true)
            {
                sourcePath = toPath;
            }
            if (!LocationStorage.IsFullPath(sourcePath))
            {
                return Path.Combine(outputFolder, sourcePath);
            }
            if (sourcePath.StartsWith(outputFolder))
            {
                return sourcePath;
            }
            return Path.Combine(outputFolder, GetRelativePath(sourcePath));
        }

        public string Create(IFilePath sourcePath, string fileName, string outputFolder)
        {
            var fullPath = FilePath.GetFilePath(sourcePath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = sourcePath.Name;
            }
            if (Mapper?.TryGet(fullPath, out var toPath) == true)
            {
                fullPath = toPath;
            }
            var sourceFolder = Path.GetDirectoryName(fullPath);
            if (!LocationStorage.IsFullPath(fullPath))
            {
                return Path.Combine(outputFolder, sourceFolder, LocationStorage.CreateSafeFileName(fileName));
            }
            if (sourceFolder?.StartsWith(outputFolder) == true)
            {
                return Path.Combine(sourceFolder, LocationStorage.CreateSafeFileName(fileName));
            }
            return Path.Combine(outputFolder, GetRelativePath(sourceFolder), LocationStorage.CreateSafeFileName(fileName));
        }

        public IBundleChunk Repack(IEnumerable<string> fileItems)
        {
            return new BundleChunk(_entranceItems, [.. fileItems], _effectiveCount);
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
