using System;
using System.Collections.Generic;
using ZoDream.BundleExtractor.Producers;

namespace ZoDream.BundleExtractor.Platforms
{
    public interface IPlatformScheme
    {
        public string Root { get; }

        public IProducerScheme Producer { get; }
        public bool TryLoad(IEnumerable<string> fileItems);


    }
}
