using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.PlayerAsset
{
    public class PlayerAssetScheme(IBundleSource source) : IBundleSource, IBundleHandler
    {

        private readonly Dictionary<IFilePath, Tuple<long, long>> _maps = [];

        public uint Count => source.Count;

        public uint Analyze(CancellationToken token = default)
        {
            foreach (var item in source.GetFiles("playerassets.assets"))
            {
                var mapFile = item[..^6] + "json";
                if (!source.Exists(mapFile))
                {
                    continue;
                }
                using var fs = source.OpenRead(new FilePath(mapFile));
                using var doc = JsonDocument.Parse(fs);
                var root = doc.RootElement;
                if (!root.TryGetProperty("data", out var data))
                {
                    continue;
                }
                var end = new FileInfo(item).Length;
                foreach (var node in data.EnumerateArray().Reverse())
                {
                    var key = node.GetProperty("key").GetString();
                    var offset = node.GetProperty("offset").GetInt64();
                    if (!string.IsNullOrEmpty(key))
                    {
                        _maps.TryAdd(new FileEntryPath(item, key), new Tuple<long, long>(offset, end - offset));
                    }
                    end = offset;
                }
            }
            return (uint)_maps.Count;
        }

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            foreach (var item in source.GetFiles("playerassets.assets"))
            {
                var mapFile = item[..^6] + "json";
                if (!source.Exists(mapFile))
                {
                    continue;
                }
                using var fs = source.OpenRead(new FilePath(mapFile));
                using var doc = JsonDocument.Parse(fs);
                var root = doc.RootElement;
                if (!root.TryGetProperty("data", out var data))
                {
                    continue;
                }
                using var bfs = source.OpenRead(new FilePath(item));
                var end = bfs.Length;
                foreach (var node in data.EnumerateArray().Reverse())
                {
                    var key = node.GetProperty("key").GetString();
                    var offset = node.GetProperty("offset").GetInt64();

                    if (!string.IsNullOrEmpty(key))
                    {
                        bfs.Take(offset, end - offset).SaveAs(Path.Combine(folder, key));
                    }
                    end = offset;
                }
            }
        }

        public void Dispose()
        {
            _maps.Clear();
        }

        public bool Exists(string filePath)
        {
            return source.Exists(filePath);
        }

        public IEnumerable<string> FindFiles(string folder, string fileName)
        {
            return source.FindFiles(folder, fileName);
        }

        public IEnumerable<string> GetDirectories(params string[] searchPatternItems)
        {
            return source.GetDirectories(searchPatternItems);
        }

        public IEnumerable<string> GetFiles(params string[] searchPatternItems)
        {
            foreach (var item in _maps.Keys)
            {
                if (searchPatternItems.Length == 0)
                {
                    yield return item.FullPath;
                    continue;
                }
                if (BundleStorage.IsMatch(Path.GetFileName(item.Name), searchPatternItems))
                {
                    yield return item.FullPath;
                    continue;
                }
            }
            foreach (var item in source.GetFiles(searchPatternItems))
            {
                if (item.EndsWith("playerassets.assets") || item.EndsWith("playerassets.json"))
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

        public Stream OpenRead(IFilePath filePath)
        {
            if (filePath is IEntryPath o && _maps.TryGetValue(filePath, out var data))
            {
                return new PartialStream(source.OpenRead(new FilePath(new FilePath(o.FilePath))), data.Item1, data.Item2, false);
            }
            return source.OpenRead(filePath);
        }

        public Stream OpenWrite(IFilePath filePath)
        {
            return source.OpenWrite(filePath);
        }
    }
}
