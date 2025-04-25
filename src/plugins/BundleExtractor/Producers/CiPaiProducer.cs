using ZoDream.BundleExtractor.Unity.Scanners;
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

        public IBundleSerializer GetSerializer(IBundleOptions options)
        {
            return new BundleSerializer(Engines.UnityEngine.Converters);
        }

        public IBundleStorage GetStorage(IBundleOptions options)
        {
            return new BundleStorage();
        }
    }
}
