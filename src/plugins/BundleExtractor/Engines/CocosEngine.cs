using System;
using System.Collections.Generic;
using System.Linq;
using ZoDream.BundleExtractor.Cocos;
using ZoDream.BundleExtractor.Producers;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Engines
{
    public class CocosEngine : IBundleEngine
    {

        internal const string EngineName = "Cocos";

        public string AliasName => EngineName;

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
            if (options.Producer == PaperProducer.ProducerName)
            {
                return new BlowfishReader(fileItems);
            }
            throw new NotImplementedException();
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            if (fileItems.Glob("cocos").Any())
            {
                options.Engine = EngineName;
                return true;
            }
            return false;
        }

        public bool IsExclude(IBundleOptions options, string fileName)
        {
            return false;
        }
    }
}
