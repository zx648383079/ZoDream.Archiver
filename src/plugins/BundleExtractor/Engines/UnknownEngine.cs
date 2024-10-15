using System;
using System.Collections.Generic;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Engines
{
    public class UnknownEngine : IBundleEngine
    {
        public IEnumerable<IBundleChunk> EnumerateChunk()
        {
            return [];
        }

        public IBundleReader OpenRead(IBundleChunk fileItems)
        {
            return null;
        }

        public bool TryLoad(IEnumerable<string> fileItems)
        {

            return true;
        }
    }
}
