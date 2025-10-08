using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Bundle.Storage;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.YooAsset
{
    public class YooAssetScheme(IBundleSource source) : IBundleSource
    {
        private readonly IBundleMapper _mapper = new BundleMapper();

        public uint Count => source.Count;

        public bool IsMatch()
        {
            var buffer = new byte[4];
            foreach (var item in source.GetFiles("*.bytes").Take(5))
            {
                using var fs = File.OpenRead(item);
                if (fs.Length < 10)
                {
                    continue;
                }
                fs.ReadExactly(buffer);
                fs.Seek(0, SeekOrigin.Begin);
                if (BitConverter.ToUInt32(buffer) == YooAssetReader.Signature)
                {
                    return true;
                }
            }
            return false;
        }

        public uint Analyze(CancellationToken token = default)
        {
            var res = source.Analyze(token);
            var buffer = new byte[4];
            var hotfixPath = string.Empty;
            var lastCheck = 0L;
            foreach (var item in source.GetFiles("*.bytes"))
            {
                var fs = File.OpenRead(item);
                if (fs.Length < 10)
                {
                    fs.Dispose();
                    continue;
                }
                fs.ReadExactly(buffer);
                fs.Seek(0, SeekOrigin.Begin);
                if (BitConverter.ToUInt32(buffer) != YooAssetReader.Signature)
                {
                    fs.Dispose();
                    continue;
                }
                if (!item.Contains("ManifestFiles"))
                {
                    Add(new YooAssetReader(fs, item, YooAssetType.Apk).Read());
                    continue;
                }
                var size = fs.Length;
                fs.Dispose();
                if (string.IsNullOrEmpty(hotfixPath) || lastCheck < size)
                {
                    hotfixPath = item;
                    lastCheck = size;
                }
            }
            if (!string.IsNullOrEmpty(hotfixPath))
            {
                Add(new YooAssetReader(File.OpenRead(hotfixPath), hotfixPath,
                    YooAssetType.Hotfix).Read());
            }
            return res;
        }

        private void Add(IEnumerable<KeyValuePair<string, string>> items)
        {
            foreach (var item in items)
            {
                if (!source.Exists(item.Key))
                {
                    continue;
                }
                _mapper.Add(item.Value, item.Key);
            }
        }

        public IEnumerable<string> GetDirectories(params string[] searchPatternItems)
        {
            return source.GetDirectories(searchPatternItems);
        }

        public IEnumerable<YooAssetReader> Create(string folder)
        {
            var buffer = new byte[4];
            var hotfixPath = string.Empty;
            var lastCheck = 0L;
            foreach (var item in Directory.GetFiles(folder, "*.bytes", SearchOption.AllDirectories))
            {
                var fs = File.OpenRead(item);
                fs.ReadExactly(buffer);
                fs.Seek(0, SeekOrigin.Begin);
                if (BitConverter.ToUInt32(buffer) != YooAssetReader.Signature)
                {
                    fs.Dispose();
                    continue;
                }
                if (!item.Contains("ManifestFiles"))
                {
                    yield return new YooAssetReader(fs, item, YooAssetType.Apk);
                    continue;
                }
                var size = fs.Length;
                fs.Dispose();
                if (string.IsNullOrEmpty(hotfixPath) || lastCheck < size)
                {
                    hotfixPath = item;
                    lastCheck = size;
                }
            }
            if (!string.IsNullOrEmpty(hotfixPath))
            {
                yield return new YooAssetReader(File.OpenRead(hotfixPath), hotfixPath, 
                    YooAssetType.Hotfix);
            }
        }

        public IEnumerable<string> GetFiles(params string[] searchPatternItems)
        {
            foreach (var item in _mapper.Keys)
            {
                if (searchPatternItems.Length == 0)
                {
                    yield return item;
                    continue;
                }
                if (BundleStorage.IsMatch(Path.GetFileName(item), searchPatternItems))
                {
                    yield return item;
                    continue;
                }
            }
            //return source.GetFiles(searchPatternItems);
        }

        public string GetRelativePath(string filePath)
        {
            //if (_mapper.TryGet(filePath, out var res))
            //{
            //    return res;//source.GetRelativePath(res);
            //}
            return source.GetRelativePath(filePath);
        }

        public IEnumerable<string> Glob(params string[] searchPatternItems)
        {
            return source.Glob(searchPatternItems);
        }

        public bool Exists(string filePath)
        {
            if (LocationStorage.IsFullPath(filePath))
            {
                return source.Exists(filePath);
            }
            return _mapper.TryGet(filePath, out _);
        }
        public IEnumerable<string> FindFiles(string folder, string fileName)
        {
            if (LocationStorage.IsFullPath(folder))
            {
                foreach (var item in source.FindFiles(folder, fileName))
                {
                    yield return item;
                }
            } else
            {
                foreach (var item in _mapper.Keys)
                {
                    if (item.StartsWith(folder) && item.LastIndexOf(fileName) > folder.Length)
                    {
                        yield return item;
                    }
                }
            }
        }

        public Stream OpenRead(IFilePath filePath)
        {
            if (_mapper.TryGet(filePath.FullPath, out var res))
            {
                return source.OpenRead(FilePath.Parse(res));
            }
            return source.OpenRead(filePath);
        }

        public Stream OpenWrite(IFilePath filePath)
        {
            return source.OpenWrite(filePath);
        }
    }
}
