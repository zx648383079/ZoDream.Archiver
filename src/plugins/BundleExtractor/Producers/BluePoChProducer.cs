using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class BluePoChProducer : IBundleProducer
    {
        public string AliasName => "BLUEPOCH";
    

        public IBundleElementScanner GetScanner(IBundleOptions options)
        {
            return new BluePoChElementScanner(options.Package);
        }

        public IBundleStorage GetStorage(IBundleOptions options)
        {
            return new BluePoChElementScanner(options.Package);
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
