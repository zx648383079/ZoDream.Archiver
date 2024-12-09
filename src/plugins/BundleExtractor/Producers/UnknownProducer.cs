using System.Collections.Generic;
using ZoDream.BundleExtractor.Engines;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.Scanners;
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
            if (options.Engine == UnityEngine.EngineName)
            {
                return new OtherBundleElementScanner(options.Package);
            }
            return new BundleElementScanner();
        }

        public IBundleStorage GetStorage(IBundleOptions options)
        {
            if (options.Engine == UnityEngine.EngineName)
            {
                return new OtherBundleElementScanner(options.Package);
            }
            return new BundleStorage();
        }
    }
}
