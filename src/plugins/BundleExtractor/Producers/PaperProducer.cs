﻿using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class PaperProducer : IBundleProducer
    {
        internal const string ProducerName = "Paper";

        public string AliasName => ProducerName;

        public IBundleElementScanner GetScanner(IBundleOptions options)
        {
            return new PaperElementScanner(options.Package);
        }

        public IBundleStorage GetStorage(IBundleOptions options)
        {
            return new PaperElementScanner(options.Package);
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }

        
    }
}
