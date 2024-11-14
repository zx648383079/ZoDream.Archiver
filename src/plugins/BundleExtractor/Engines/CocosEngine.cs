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

        public const string EngineName = "Cocos";

        public IEnumerable<IBundleChunk> EnumerateChunk(IBundleSource fileItems)
        {
            return fileItems.Select(i => new BundleChunk(i));
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
    }
}
