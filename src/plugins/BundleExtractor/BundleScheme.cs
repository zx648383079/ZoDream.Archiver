using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Logging;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme : IBundleScheme
    {
        public IBundleHandler? Load(IBundleSource fileItems, IArchiveOptions? options = null)
        {
            var option = options is IBundleOptions o ? o : new BundleOptions(options);
            LoadEnvironment(fileItems, option);
            var logger = Service.Get<ILogger>();
            if (string.IsNullOrWhiteSpace(option.Platform) 
                || string.IsNullOrWhiteSpace(option.Engine))
            {
                logger?.Warning($"Platform is unkown or Engine is unknown");
                return null;
            }
            logger?.Info($"Platform: {option.Platform}" );
            return new BundleReader(fileItems, option, this);
        }

        public IBundleOptions? TryLoad(IBundleSource fileItems)
        {
            var option = new BundleOptions();
            LoadEnvironment(fileItems, option);
            return option;
        }

        public void LoadEnvironment(IBundleSource fileItems, IBundleOptions option)
        {
            if (!string.IsNullOrWhiteSpace(option.Package))
            {
                LoadWithPackage(option);
            }
            if (string.IsNullOrWhiteSpace(option.Platform))
            {
                TryGet<IBundlePlatform>(fileItems, option);
            }
            if (string.IsNullOrWhiteSpace(option.Producer))
            {
                TryGet<IBundleProducer>(fileItems, option);
            }
            if (string.IsNullOrWhiteSpace(option.Engine))
            {
                TryGet<IBundleEngine>(fileItems, option);
            }
        }

    }
}
