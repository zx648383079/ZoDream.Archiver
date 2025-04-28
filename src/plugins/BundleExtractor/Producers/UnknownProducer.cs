using System.Collections.Generic;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.Shared.Bundle;
using UnityE = ZoDream.BundleExtractor.Engines.UnityEngine;

namespace ZoDream.BundleExtractor.Producers
{
    public class UnknownProducer : IBundleProducer
    {
        internal const string CheckKey = "is_unknown";
        public string AliasName => string.Empty;
        public IList<DependencyDictionary> DependencyItems { get; private set; } = [];

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }

        public IBundleSerializer GetSerializer(IBundleOptions options)
        {
            if (options.Engine == UnityE.EngineName)
            {
                return new BundleSerializer(UnityConverter.Converters);
            }
            return new BundleSerializer();
        }

        public IBundleStorage GetStorage(IBundleOptions options)
        {
            if (options.Engine == UnityE.EngineName)
            {
                return new OtherBundleElementScanner(
                    options.Package ?? string.Empty,
                    options
                 );
            }
            return new BundleStorage();
        }
    }
}
