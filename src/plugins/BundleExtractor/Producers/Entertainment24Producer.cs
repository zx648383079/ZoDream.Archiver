using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class Entertainment24Producer : IBundleProducer
    {
        public string AliasName => "24 Entertainment";

        public IBundleStorage GetStorage(IBundleOptions options)
        {
            return new Entertainment24ElementScanner(options.Package ?? string.Empty);
        }

        public IBundleElementScanner GetScanner(IBundleOptions options)
        {
            return new Entertainment24ElementScanner(options.Package ?? string.Empty);
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
