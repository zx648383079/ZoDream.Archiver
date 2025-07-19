using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Bundle.Storage;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Unity.YooAsset
{
    public class YooAssetScheme : IBundleArchiveScheme
    {

        public static bool TryGet(IBundleSource items, [NotNullWhen(true)] out IBundleMapper? mapper)
        {
            var res = new BundleMapper();
            var buffer = new byte[4];
            var hotfixPath = string.Empty;
            var lastCheck = 0L;
            foreach (var item in items.Glob("*.bytes"))
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
                    new YooAssetReader(fs, item, YooAssetType.Apk).Write(res);
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
                    YooAssetType.Hotfix).Write(res);
            }
            mapper = res;
            return res.Count > 0;
        }

        public IEnumerable<YooAssetReader> GetFiles(string folder)
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
        

        public IArchiveReader? Open(IBundleBinaryReader reader, IFilePath sourcePath, IArchiveOptions? options = null)
        {
            throw new NotImplementedException();
        }
    }
}
