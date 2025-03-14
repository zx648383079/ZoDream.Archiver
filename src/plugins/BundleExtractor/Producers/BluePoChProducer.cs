﻿using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Producers
{
    public class BluePoChProducer : IBundleProducer
    {
        public string AliasName => "BLUEPOCH";
    

        public IBundleElementScanner GetScanner(IBundleOptions options)
        {
            return new BluePoChElementScanner(options.Package ?? string.Empty);
        }

        public IBundleStorage GetStorage(IBundleOptions options)
        {
            return new BluePoChElementScanner(options.Package ?? string.Empty);
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
