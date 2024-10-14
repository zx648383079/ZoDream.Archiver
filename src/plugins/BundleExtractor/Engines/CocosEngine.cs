using System;
using System.Collections.Generic;
using ZoDream.BundleExtractor.Cocos;
using ZoDream.BundleExtractor.Producers;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Engines
{
    public class CocosEngine(IBundlePlatform platform) : IBundleEngine
    {
        public IBundleReader OpenRead(IEnumerable<string> fileItems)
        {
            if (platform.Producer is PaperProducer)
            {
                return new BlowfishReader(fileItems, "fd1c1b2f34a0d1d246be3ba9bc5af022e83375f315a0216085d3013a");
            }
            throw new NotImplementedException();
        }

        public bool TryLoad(IEnumerable<string> fileItems)
        {
            throw new NotImplementedException();
        }
    }
}
