using System;
using System.Collections.Generic;
using System.Linq;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Engines
{
    public class RPGMakerEngine : IBundleEngine
    {
        internal const string EngineName = "RPG Maker";

        public string AliasName => EngineName;
        public IBundleSplitter CreateSplitter(IBundleOptions options)
        {
            return new BundleSplitter(options is IBundleExtractOptions o ? Math.Max(o.MaxBatchCount, 1) : 100);
        }

        public IBundleSource Unpack(IBundleSource fileItems, IBundleOptions options)
        {
            return fileItems;
        }
        public IDependencyBuilder GetBuilder(IBundleOptions options)
        {
            return new DependencyBuilder(options is IBundleExtractOptions o ? o.DependencySource : string.Empty);
        }
        public IBundleHandler CreateHandler(IBundleChunk fileItems, IBundleOptions options)
        {
            return null;
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            if (fileItems.GetFiles("*.rpgmv*", "*.rgss*").Any())
            {
                options.Engine = EngineName;
                return true;
            }
            return false;
        }

    }
}
