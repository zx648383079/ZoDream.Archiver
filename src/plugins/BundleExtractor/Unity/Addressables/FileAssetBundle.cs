using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Bundle.Storage;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Addressables
{
    public class FileAssetBundle(IBundleSource source) : IBundleSource, IAssetBundleSource, IBundleHandler
    {
        public string AliasName => "com.unity.addressables";

        private readonly IBundleMapper _mapper = new BundleMapper();

        public uint Count => source.Count;

        public bool IsMatch()
        {
            return source.GetDirectories("com.unity.addressables").Any();
        }

        public uint Analyze(CancellationToken token = default)
        {
            var res = source.Analyze(token);
            _mapper.Clear();
            AnalyzePath();
            return res;
        }
        /// <summary>
        /// 根据路径直接判断
        /// </summary>
        private void AnalyzePath()
        {
            var targetFolder = source.GetDirectories("Addressable").FirstOrDefault();
            foreach (var item in source.GetDirectories("com.unity.addressables"))
            {
                var mapFile = Path.Combine(item, "file.json");
                if (!source.Exists(mapFile))
                {
                    continue;
                }
                using var fs = source.OpenRead(new FilePath(mapFile));
                using var doc = JsonDocument.Parse(fs);
                var root = doc.RootElement;
                /// {version: string, buildTime: string, 
                /// bundles:{bundleName: string, readableName: string, hash: string, fileHash: string, size: long, bundleType: Remote}[]
                /// }
                if (!root.TryGetProperty("bundles", out var data))
                {
                    continue;
                }
                foreach (var bundle in data.EnumerateArray())
                {
                    var src = Path.Combine(targetFolder,
                        bundle.GetProperty("bundleName").GetString(),
                        bundle.GetProperty("hash").GetString(),
                        "__data"
                        );
                    if (!source.Exists(src))
                    {
                        continue;
                    }
                    //if (LocationStorage.HashFile(src) != bundle.GetProperty("fileHash").GetString())
                    //{
                    //    continue;
                    //}
                    _mapper.Add(bundle.GetProperty("readableName").GetString(), src);
                }
            }
        }
        /// <summary>
        /// 根据 hash 值判断
        /// </summary>
        private void AnalyzeHash()
        {
            var maps = new Dictionary<string, string>();
            foreach (var item in source.GetDirectories("com.unity.addressables"))
            {
                var mapFile = Path.Combine(item, "file.json");
                if (!source.Exists(mapFile))
                {
                    continue;
                }
                using var fs = source.OpenRead(new FilePath(mapFile));
                using var doc = JsonDocument.Parse(fs);
                var root = doc.RootElement;
                /// {version: string, buildTime: string, 
                /// bundles:{bundleName: string, readableName: string, hash: string, fileHash: string, size: long, bundleType: Remote}[]
                /// }
                if (!root.TryGetProperty("bundles", out var data))
                {
                    continue;
                }
                foreach (var bundle in data.EnumerateArray())
                {
                    maps.Add(
                        bundle.GetProperty("fileHash").GetString(), 
                        bundle.GetProperty("readableName").GetString());
                }
            }

            foreach (var item in source.GetFiles("__data"))
            {
                if (maps.TryGetValue(LocationStorage.HashFile(item), out var target))
                {
                    _mapper.Add(target, item);
                }
            }
        }

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            _mapper.Clear();
            AnalyzeHash();
            foreach (var item in _mapper.Keys)
            {
                if (!LocationStorage.TryCreate(Path.Combine(folder, item), mode, out var outputPath))
                {
                    continue;
                }
                using var bfs = source.OpenRead(new FilePath(item));
                bfs.SaveAs(outputPath);
            }
        }

        public void Dispose()
        {
            _mapper.Clear();
        }

        public bool Exists(string filePath)
        {
            if (LocationStorage.IsFullPath(filePath))
            {
                return source.Exists(filePath);
            }
            return _mapper.TryGet(filePath, out _);
        }

        public IEnumerable<string> GetDirectories(params string[] searchPatternItems)
        {
            return source.GetDirectories(searchPatternItems);
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
            foreach (var item in source.GetFiles(searchPatternItems))
            {
                if (Path.GetFileName(item) == "__data")
                {
                    continue;
                }
                yield return item;
            }
        }

        public string GetRelativePath(string filePath)
        {
            return source.GetRelativePath(filePath);
        }

        public IEnumerable<string> Glob(params string[] searchPatternItems)
        {
            return source.Glob(searchPatternItems);
        }

        public IEnumerable<string> FindFiles(string folder, string fileName)
        {
            if (LocationStorage.IsFullPath(folder))
            {
                foreach (var item in source.FindFiles(folder, fileName))
                {
                    yield return item;
                }
            }
            else
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
