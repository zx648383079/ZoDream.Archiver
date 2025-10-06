using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Bundle.Storage;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.YooAsset
{
    public class YooAssetScheme(IBundleSource source) : IBundleSource
    {
        private readonly IBundleMapper _mapper = new BundleMapper();

        public uint Count => source.Count;


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
                    new YooAssetReader(fs, item, YooAssetType.Apk).Write(_mapper);
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
                new YooAssetReader(File.OpenRead(hotfixPath), hotfixPath,
                    YooAssetType.Hotfix).Write(_mapper);
            }
            return res;
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
            return source.GetFiles(searchPatternItems);
        }

        public string GetRelativePath(string filePath)
        {
            //if (_mapper.TryGet(filePath, out var res))
            //{
            //    return source.GetRelativePath(res);
            //}
            return source.GetRelativePath(filePath);
        }

        public IEnumerable<string> Glob(params string[] searchPatternItems)
        {
            return source.Glob(searchPatternItems);
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
