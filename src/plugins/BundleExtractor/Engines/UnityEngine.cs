using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ZoDream.BundleExtractor.Platforms;
using ZoDream.BundleExtractor.Unity;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Engines
{
    public class UnityEngine(IEntryService service) : IBundleEngine
    {
        internal const string EngineName = "Unity";
        
        public string AliasName => EngineName;

        private const string Il2CppGameAssemblyName = "libil2cpp.so";
        private const string AndroidAssemblyName = "libunity.so";
        private const string WindowsAssemblyName = "UnityPlayer.dll";

        public IEnumerable<IBundleChunk> EnumerateChunk(IBundleSource fileItems, IBundleOptions options)
        {
            if (options is not IBundleExtractOptions o)
            {
                return fileItems.EnumerateChunk(100);
            }
            if (o.OnlyDependencyTask || string.IsNullOrWhiteSpace(o.DependencySource))
            {
                return fileItems.EnumerateChunk(Math.Max(o.MaxBatchCount, 1));
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
            return fileItems.EnumerateChunk(dict);
        }
        public IDependencyBuilder GetBuilder(IBundleOptions options)
        {

            return new DependencyBuilder(options is IBundleExtractOptions o && o.OnlyDependencyTask ? o.DependencySource : string.Empty);
        }
        public IBundleReader OpenRead(IBundleChunk fileItems, IBundleOptions options)
        {
            service.AddIf<UnityBundleScheme>();
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

        public bool IsExclude(IBundleOptions options, string fileName)
        {
            if (!string.IsNullOrWhiteSpace(options.Entrance) &&
                fileName.StartsWith(options.Entrance))
            {
                if (options.Platform == AndroidPlatformScheme.PlatformName)
                {
                    return !fileName.StartsWith(Path.Combine(options.Entrance,
                        "assets"));
                }
            }
            var i = fileName.LastIndexOf('.');
            if (i < 0)
            {
                return false;
            }
            return fileName[(i + 1)..].ToLower() switch
            {
                "xml" or "dex" or "so" or "kotlin_metadata" or "dylib" => true,
                _ => false
            };
        }
    }
}
