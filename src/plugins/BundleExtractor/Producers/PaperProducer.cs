using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class PaperProducer : IBundleProducer
    {
        public const string ProducerName = "Paper";
        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
