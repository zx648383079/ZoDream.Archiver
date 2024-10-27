using System;
using System.Collections.Generic;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Producers
{
    public class PaperProducer : IBundleProducer
    {
        
        public bool TryLoad(IEnumerable<string> fileItems)
        {
            return false;
        }
    }
}
