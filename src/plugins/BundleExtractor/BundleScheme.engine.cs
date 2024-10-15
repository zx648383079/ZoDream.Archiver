using ZoDream.Shared.Interfaces;
using System.Collections.Generic;
using ZoDream.BundleExtractor.Engines;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme
    {
        internal static IBundleEngine GetEngine(
            IBundlePlatform platform,
            IEnumerable<string> fileItems)
        {
            IBundleEngine[] items = [
                new UnityEngine(platform),
                new CocosEngine(platform)
            ];
            foreach (var item in items)
            {
                if (item.TryLoad(fileItems))
                {
                    return item;
                }
            }
            return new UnknownEngine();
        }

    }
}
