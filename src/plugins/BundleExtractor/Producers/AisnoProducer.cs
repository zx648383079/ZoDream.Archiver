using System;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class AisnoProducer : IBundleProducer
    {
        public string AliasName => "AISNO";

        

        public IBundleStorage GetStorage(IBundleOptions options)
        {
            return AisnoElementScanner(options.Package ?? string.Empty);
        }

        private IBundleStorage AisnoElementScanner(string v)
        {
            throw new NotImplementedException();
        }

        public IBundleSerializer GetSerializer(IBundleOptions options)
        {
            return new BundleSerializer(UnityConverter.Converters);
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
