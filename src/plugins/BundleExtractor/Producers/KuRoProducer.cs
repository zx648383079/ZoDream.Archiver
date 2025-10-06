using System;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class KuRoProducer : IBundleProducer
    {
        internal const string ProducerName = "KuRo Games";

        public string AliasName => ProducerName;

        public IBundleSerializer CreateSerializer(IBundleOptions options)
        {
            throw new NotImplementedException();
        }

        public IBundleParser CreateParser(IBundleOptions options)
        {
            throw new NotImplementedException();
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
