﻿using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class MiHoYoProducer : IBundleProducer
    {
        internal const string ProducerName = "MiHoYo";

        public string AliasName => ProducerName;

        public IBundleElementScanner GetScanner(IBundleOptions options)
        {
            return new MiHoYoElementScanner(options.Package);
        }

        public IBundleStorage GetStorage(IBundleOptions options)
        {
            return new MiHoYoElementScanner(options.Package);
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
