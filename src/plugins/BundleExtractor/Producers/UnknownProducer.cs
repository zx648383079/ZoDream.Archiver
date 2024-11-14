using BundleExtractor.Models;
using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class UnknownProducer : IBundleProducer
    {
        public IList<DependencyDictionary> DependencyItems { get; private set; } = [];

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
