using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class IceConeProducer : IBundleProducer
    {
        public string AliasName => "IceCone Studio";

        public IBundleSerializer GetSerializer(IBundleOptions options)
        {
            return new BundleSerializer(UnityConverter.Converters);
        }

        public IBundleStorage GetStorage(IBundleOptions options)
        {
            return new IceConeElementScanner(options.Package ?? string.Empty);
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
