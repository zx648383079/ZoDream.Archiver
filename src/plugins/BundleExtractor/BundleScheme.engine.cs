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
                UnrealEngine.EngineName => new UnrealEngine(),
                GodotEngine.EngineName => new GodotEngine(),
                KrKrEngine.EngineName => new KrKrEngine(),
                RenPyEngine.EngineName => new RenPyEngine(),
                RPGMakerEngine.EngineName => new RPGMakerEngine(),
                TyranoEngine.EngineName => new TyranoEngine(),
                _ => new UnknownEngine(),
            };
        }

        internal static bool TryGetEngine(
            IBundleSource fileItems, IBundleOptions options)
        {
            IBundleEngine[] items = [
                new UnityEngine(),
                new CocosEngine(),
                new UnrealEngine(),
                new GodotEngine(),
                new KrKrEngine(),
                new RenPyEngine(),
                new RPGMakerEngine(),
                new TyranoEngine()
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
