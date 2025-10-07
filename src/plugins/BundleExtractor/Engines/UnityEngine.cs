using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using ZoDream.BundleExtractor.Platforms;
using ZoDream.BundleExtractor.Unity;
using ZoDream.BundleExtractor.Unity.YooAsset;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Bundle.Storage;
using ZoDream.Shared.Interfaces;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Engines
{
    public class UnityEngine(IEntryService service) : IBundleEngine, IBundleFilter
    {
        internal const string EngineName = "Unity";
        
        public string AliasName => EngineName;

        private const string Il2CppGameAssemblyName = "libil2cpp.so";
        private const string AndroidAssemblyName = "libunity.so";
        private const string WindowsAssemblyName = "UnityPlayer.dll";

        public IBundleSplitter CreateSplitter(IBundleOptions options)
        {
            if (options is not IBundleExtractOptions o)
            {
                return new BundleSplitter(100);
            }
            var maxBatchCount = Math.Max(o.MaxBatchCount, 1);
            if (o.OnlyDependencyTask || string.IsNullOrWhiteSpace(o.DependencySource))
            {
                return new BundleSplitter(maxBatchCount);
            }
            if (!service.TryGet<IDependencyDictionary>(out var dict))
            {
                if (o.DependencySource.EndsWith(".json"))
                {
                    dict = LoadDependency(o.DependencySource);
                } else
                {
                    using var builder = DependencyBuilder.Load(o.DependencySource);
                    dict = builder.ToDictionary(StringComparison.OrdinalIgnoreCase);
                    if (dict is DependencyDictionary s)
                    {
                        s.FileName = o.DependencySource;
                    }
                }
                service.Add(dict);
            }
            return new BundleDependencySplitter(dict, maxBatchCount);
        }

        public IBundleSource Unpack(IBundleSource fileItems, IBundleOptions options)
        {
            return new YooAssetScheme(fileItems);
        }

        public IDependencyBuilder GetBuilder(IBundleOptions options)
        {
            return new DependencyBuilder(options is IBundleExtractOptions o && o.OnlyDependencyTask ? o.DependencySource : string.Empty);
        }
        public IBundleHandler CreateHandler(IBundleChunk fileItems, IBundleOptions options)
        {
            service.AddIf<UnityBundleScheme>();
            if (!string.IsNullOrWhiteSpace(options.Version) && Version.TryParse(options.Version, out var version, out _))
            {
                service.Add(version);
            }
            return new UnityBundleChunkReader(fileItems, service, options);
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            if (fileItems.GetFiles(AndroidAssemblyName, Il2CppGameAssemblyName).Any())
            {
                options.Platform = AndroidPlatformScheme.PlatformName;
                options.Engine = EngineName;
                return true;
            }
            if (fileItems.GetFiles(WindowsAssemblyName).Any())
            {
                options.Platform = WindowsPlatformScheme.PlatformName;
                options.Engine = EngineName;
                return true;
            }
            return false;
        }

        public IDependencyDictionary LoadDependency(string fileName)
        {
            var instance = new DependencyDictionary
            {
                FileName = Path.GetFullPath(fileName)
            };
            if (!fileName.EndsWith(".json"))
            {
                using var fs = File.OpenRead(fileName);
                LoadDependency(fs, instance);
                return instance;
            }
            instance.Entrance = Path.GetDirectoryName(fileName)!;
            var items = JsonSerializer.Deserialize<UTY_DependencyFile>(File.ReadAllText(fileName));
            if (items is null)
            {
                return instance;
            }
            foreach (var item in items.assetbundles)
            {
                var cab = items.files[item.n];
                if (string.IsNullOrEmpty(fileName))
                {
                    continue;
                }
                instance.TryAdd(cab, new DependencyEntry(cab, 0, item.d.Select(i => items.files[i]).ToArray()));
            }
            return instance;
        }

        private void LoadDependency(Stream input, DependencyDictionary instance)
        {
            using var reader = new BinaryReader(input);
            instance.Entrance = reader.ReadString();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var cab = reader.ReadString();
                var path = reader.ReadString();
                var offset = reader.ReadInt64();
                var depCount = reader.ReadInt32();
                var dependencies = new string[depCount];
                for (int j = 0; j < depCount; j++)
                {
                    dependencies[i] = reader.ReadString();
                }
                instance.TryAdd(cab, new DependencyEntry(path, offset, dependencies));
            }
        }

        public bool IsMatch(IFilePath filePath)
        {
            var options = service.Get<IBundleOptions>();
            if (!string.IsNullOrWhiteSpace(options.Entrance) &&
                filePath.FullPath.StartsWith(options.Entrance))
            {
                if (options.Platform == AndroidPlatformScheme.PlatformName)
                {
                    return !filePath.FullPath.StartsWith(Path.Combine(options.Entrance,
                        "assets"));
                }
            }
            var i = filePath.FullPath.LastIndexOf('.');
            if (i < 0)
            {
                return false;
            }
            return filePath.FullPath[(i + 1)..].ToLower() switch
            {
                "xml" or "dex" or "so" or "kotlin_metadata" or "dylib" => true,
                _ => false
            };
        }

    }
}
