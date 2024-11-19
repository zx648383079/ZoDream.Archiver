using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme
    {
        private IBundleProducer[] _producerItems = [];

        public string[] ProducerNames => GetNames(_producerItems);
    }
}
