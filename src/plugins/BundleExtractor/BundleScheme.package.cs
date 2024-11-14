using ZoDream.BundleExtractor.Engines;
using ZoDream.BundleExtractor.Platforms;
using ZoDream.BundleExtractor.Producers;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme
    {
        public static void LoadWithPackage(IBundleOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Package))
            {
                return;
            }
            if (options.Package.EndsWith(".qjnn"))
            {
                options.Platform ??= AndroidPlatformScheme.PlatformName;
                options.Engine = CocosEngine.EngineName;
                options.Producer = PaperProducer.ProducerName;
                return;
            }
            if (options.Package.StartsWith("com.papegames."))
            {
                options.Platform ??= AndroidPlatformScheme.PlatformName;
                options.Engine = UnityEngine.EngineName; ;
                options.Producer = PaperProducer.ProducerName;
                return;
            }
        }
    }
}
