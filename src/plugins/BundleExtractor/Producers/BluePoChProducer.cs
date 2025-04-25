using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class BluePoChProducer : IBundleProducer
    {
        public string AliasName => "BLUEPOCH";


        public IBundleSerializer GetSerializer(IBundleOptions options)
        {
            return new BundleSerializer(Engines.UnityEngine.Converters);
        }

        public IBundleStorage GetStorage(IBundleOptions options)
        {
            return new BluePoChElementScanner(options.Package ?? string.Empty);
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
