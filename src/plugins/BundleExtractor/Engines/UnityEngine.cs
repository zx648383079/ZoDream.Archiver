using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Engines
{
    public class UnityEngine : IBundleEngine, IOfPlatform
    {
        public UnityEngine()
        {
        }

        public UnityEngine(IBundlePlatform platform)
        {
            _platform = platform;
        }

        private IBundlePlatform? _platform;
        private IEnumerable<string>? _fileItems;
        private const string Il2CppGameAssemblyName = "libil2cpp.so";
        private const string AndroidUnityAssemblyName = "libunity.so";

        public IBundlePlatform Platform {
            set { _platform = value; }
        }

        private readonly UnityBundleScheme _scheme = new();

        public IEnumerable<IBundleChunk> EnumerateChunk()
        {
            if (_fileItems is null)
            {
                return [];
            }
            return _fileItems.Select(i => new BundleChunk(i));
        }

        public IBundleReader OpenRead(IBundleChunk fileItems)
        {
            return new UnityBundleChunkReader(fileItems, _scheme, _platform);
        }

        public bool TryLoad(IEnumerable<string> fileItems)
        {
            _fileItems = fileItems;
            foreach (var item in fileItems)
            {
                if (File.Exists(item))
                {
                    continue;
                }
                if (Directory.GetFiles(item, AndroidUnityAssemblyName,
                    SearchOption.AllDirectories).Length > 0 || 
                    Directory.GetFiles(item, Il2CppGameAssemblyName,
                    SearchOption.AllDirectories).Length > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
