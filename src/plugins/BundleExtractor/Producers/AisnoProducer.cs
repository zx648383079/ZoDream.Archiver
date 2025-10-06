using System;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class AisnoProducer : IBundleProducer
    {
        public string AliasName => "AISNO";

        

        public IBundleParser CreateParser(IBundleOptions options)
        {
            return new AisnoElementScanner(options.Package ?? string.Empty);
        }

        public IBundleSerializer CreateSerializer(IBundleOptions options)
        {
            return new BundleSerializer(UnityConverter.Converters);
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
