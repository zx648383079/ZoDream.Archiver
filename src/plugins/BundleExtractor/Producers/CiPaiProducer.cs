using ZoDream.BundleExtractor.Unity.Converters;
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
            return new BundleSerializer(UnityConverter.Converters);
        }

        public IBundleStorage GetStorage(IBundleOptions options)
        {
            return new BundleStorage();
        }
    }
}
