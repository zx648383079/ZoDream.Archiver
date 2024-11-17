using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class PaperProducer : IBundleProducer
    {
        internal const string ProducerName = "Paper";

        public string AliasName => ProducerName;
        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
