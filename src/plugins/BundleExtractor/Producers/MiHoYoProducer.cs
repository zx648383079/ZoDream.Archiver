using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class MiHoYoProducer : IBundleProducer
    {
        internal const string ProducerName = "MiHoYo";

        public string AliasName => ProducerName;

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
