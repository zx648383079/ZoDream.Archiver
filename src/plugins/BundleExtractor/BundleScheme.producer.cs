using System.Linq;
using ZoDream.BundleExtractor.Producers;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme
    {
        private readonly IBundleProducer[] _producerItems = [
            new MiHoYoProducer(),
            new PaperProducer(),
            new UnknownProducer(),
        ];

        public string[] ProducerNames => GetNames(_producerItems);
    }
}
