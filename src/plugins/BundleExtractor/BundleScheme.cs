using System.Collections.Generic;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme : IBundleScheme
    {
        public IBundleReader? Load(IEnumerable<string> fileItems, IArchiveOptions? options = null)
        {
            var source = fileItems is BundleSource s ? s : new BundleSource(fileItems);
            var option = options is IBundleOptions o ? o : new BundleOptions(options);
            LoadEnvironment(source, option);
            if (string.IsNullOrWhiteSpace(option.Platform) 
                || string.IsNullOrWhiteSpace(option.Engine))
            {
                return null;
            }
            Service.Get<ILogger>().Info(option.Platform);
            return new BundleReader(source, option, this);
        }

        public IBundleOptions? TryLoad(IEnumerable<string> fileItems)
        {
            var option = new BundleOptions();
            LoadEnvironment(fileItems is BundleSource s ? s : new BundleSource(fileItems), option);
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
