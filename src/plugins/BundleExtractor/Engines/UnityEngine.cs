using System;
using System.Collections.Generic;
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

        public IBundlePlatform Platform {
            set { _platform = value; }
        }

        private readonly UnityBundleScheme _scheme = new();

        public IEnumerable<IBundleChunk> EnumerateChunk()
        {
            throw new NotImplementedException();
        }

        public IBundleReader OpenRead(IBundleChunk fileItems)
        {
            return new UnityBundleChunkReader(fileItems, _scheme, _platform);
        }

        public bool TryLoad(IEnumerable<string> fileItems)
        {
            throw new NotImplementedException();
        }
    }
}
