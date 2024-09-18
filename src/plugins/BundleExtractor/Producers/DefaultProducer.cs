using BundleExtractor.Models;
using System;
using System.Collections.Generic;

namespace ZoDream.BundleExtractor.Producers
{
    public class DefaultProducer : IProducerScheme
    {
        public IList<DependencyDictionary> DependencyItems { get; private set; } = [];

        public bool TryLoad(IEnumerable<string> fileItems)
        {
            throw new NotImplementedException();
        }
    }
}
