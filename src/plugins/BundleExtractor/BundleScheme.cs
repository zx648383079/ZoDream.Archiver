using ZoDream.Shared.Interfaces;
using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme(ILogger logger) : IBundleScheme
    {
        public IBundleReader? Load(IEnumerable<string> fileItems, IArchiveOptions? options = null)
        {
            var source = fileItems is BundleSource s ? s : new BundleSource(fileItems);
            var option = options is IBundleOptions o ? o : new BundleOptions(options);
            Load(source, option);
            if (string.IsNullOrWhiteSpace(option.Platform) 
                || string.IsNullOrWhiteSpace(option.Engine))
            {
                return null;
            }
            logger.Info(option.Platform);
            return new BundleReader(source, option, logger);
        }

        public IBundleOptions? TryLoad(IEnumerable<string> fileItems)
        {
            var option = new BundleOptions();
            Load(fileItems is BundleSource s ? s : new BundleSource(fileItems), option);
            return option;
        }

        public void Load(IBundleSource fileItems, IBundleOptions option)
        {
            if (string.IsNullOrWhiteSpace(option.Package))
            {
                LoadWithPackage(option);
            }
            if (string.IsNullOrWhiteSpace(option.Platform))
            {
                TryGetPlatform(fileItems, option);
            }
            if (string.IsNullOrWhiteSpace(option.Producer))
            {
                TryGetProducer(fileItems, option);
            }
            if (string.IsNullOrWhiteSpace(option.Engine))
            {
                TryGetEngine(fileItems, option);
            }
        }
    }
}
