using ZoDream.Shared.Interfaces;
using ZoDream.BundleExtractor.Platforms;
using System.Collections.Generic;
using ZoDream.BundleExtractor.Producers;
using ZoDream.BundleExtractor.Engines;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme
    {
        internal static IBundleProducer GetProducer(IEnumerable<string> fileItems)
        {
            IBundleProducer[] producers = [
                new MiHoYoProducer(),
                new PaperProducer()
            ];
            foreach (var item in producers)
            {
                if (item.TryLoad(fileItems))
                {
                    return item;
                }
            }
            return new UnityProducer();
        }


    }
}
