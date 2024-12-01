using System.Collections.Generic;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class UnknownProducer : IBundleProducer
    {
        public string AliasName => string.Empty;
        public IList<DependencyDictionary> DependencyItems { get; private set; } = [];

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }

        public IBundleElementScanner GetScanner(IBundleOptions options)
        {
            return new BundleElementScanner();
        }
    }
}
