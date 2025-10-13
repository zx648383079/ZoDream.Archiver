using System;
using System.Linq;
using ZoDream.BundleExtractor.Xna;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Engines
{
    public class XnaEngine(IEntryService service) : IBundleEngine
    {
        internal const string EngineName = "XNA";

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
            return new ResourceReader(fileItems, new XnbScheme(service, options), service);
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            if (fileItems.GetFiles("*.xnb").Any())
            {
                options.Engine = EngineName;
                return true;
            }
            return false;
        }
    }
}
