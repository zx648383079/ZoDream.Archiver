using System;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class KuRoProducer : IBundleProducer
    {
        internal const string ProducerName = "KuRo Games";

        public string AliasName => ProducerName;

        public IBundleSerializer GetSerializer(IBundleOptions options)
        {
            throw new NotImplementedException();
        }

        public IBundleStorage GetStorage(IBundleOptions options)
        {
            throw new NotImplementedException();
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
