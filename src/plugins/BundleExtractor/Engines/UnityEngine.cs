using System;
using System.Collections.Generic;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Engines
{
    public class UnityEngine(IBundlePlatform platform) : IBundleEngine
    {
        private readonly UnityBundleScheme _scheme = new();

        public IBundleReader OpenRead(IEnumerable<string> fileItems)
        {
            return new UnityBundleChunkReader(fileItems, _scheme, platform);
        }

        public bool TryLoad(IEnumerable<string> fileItems)
        {
            throw new NotImplementedException();
        }
    }
}
