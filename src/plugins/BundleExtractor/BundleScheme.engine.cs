using ZoDream.BundleExtractor.Engines;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme
    {
        internal static IBundleEngine CreateEngine(IBundleOptions options)
        {
            return options.Engine switch
            {
                CocosEngine.EngineName => new CocosEngine(),
                UnityEngine.EngineName => new UnityEngine(),
                _ => new UnknownEngine(),
            };
        }

        internal static bool TryGetEngine(
            IBundleSource fileItems, IBundleOptions options)
        {
            IBundleEngine[] items = [
                new UnityEngine(),
                new CocosEngine()
            ];
            foreach (var item in items)
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
