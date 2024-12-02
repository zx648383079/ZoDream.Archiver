using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class CiPaiProducer : IBundleProducer
    {
        public string AliasName => "Ci Pai Studio";
        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }

        public IBundleElementScanner GetScanner(IBundleOptions options)
        {
            return new BundleElementScanner();
        }

        public IBundleStorage GetStorage(IBundleOptions options)
        {
            return new BundleStorage();
        }
    }
}
