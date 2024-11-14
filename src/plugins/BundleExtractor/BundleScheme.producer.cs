using ZoDream.BundleExtractor.Producers;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme
    {
        internal static IBundleProducer CreateProducer(IBundleOptions options)
        {
            return options.Producer switch
            {
                MiHoYoProducer.ProducerName => new MiHoYoProducer(),
                PaperProducer.ProducerName => new PaperProducer(),
                _ => new UnknownProducer(),
            };
        }

        internal static bool TryGetProducer(IBundleSource fileItems, IBundleOptions options)
        {
            IBundleProducer[] producers = [
                new MiHoYoProducer(),
                new PaperProducer()
            ];
            foreach (var item in producers)
            {
                if (item.TryLoad(fileItems, options))
                {
                    return true;
                }
            }
            return false;
        }


    }
}
