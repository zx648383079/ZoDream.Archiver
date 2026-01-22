using System;
using System.Linq;
using ZoDream.BundleExtractor.IFAction;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Engines
{
    public class IFActionEngine(IEntryService service) : IBundleEngine
    {

        internal const string EngineName = "iFAction";

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
            return new IFActionScheme(fileItems, service, options);
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            if (fileItems.Glob("iFCon", "iF2D.dll", "*.ifMap").Any())
            {
                options.Engine = EngineName;
                return true;
            }
            return false;
        }


    }
}
