using System;
using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Engines
{
    public class UnknownEngine : IBundleEngine
    {
        public string AliasName => string.Empty;
        public IEnumerable<IBundleChunk> EnumerateChunk(IBundleSource fileItems, IBundleOptions options)
        {
            return fileItems.EnumerateChunk(options is IBundleExtractOptions o ? Math.Max(o.MaxBatchCount, 1) : 100);
        }
        public IDependencyBuilder GetBuilder(IBundleOptions options)
        {
            return new DependencyBuilder(options is IBundleExtractOptions o ? o.DependencySource : string.Empty);
        }
        public IBundleReader OpenRead(IBundleChunk fileItems, IBundleOptions options)
        {
            return null;
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
        public bool IsExclude(IBundleOptions options, string fileName)
        {
            return false;
        }

    }
}
