using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class HyperGryPhProducer : IBundleProducer
    {
        internal const string ProducerName = "HyperGryPh";

        public string AliasName => ProducerName;

        public IBundleElementScanner GetScanner(IBundleOptions options)
        {
            return new HyperGryPhElementScanner(options.Package);
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
