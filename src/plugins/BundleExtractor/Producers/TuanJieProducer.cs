using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class TuanJieProducer : IBundleProducer
    {
        internal const string ProducerName = "Tuan Jie";

        public string AliasName => ProducerName;

        public IBundleSerializer GetSerializer(IBundleOptions options)
        {
            return new BundleSerializer([
                .. UnityConverter.Converters,
                new TuanJieElementScanner(options.Package ?? string.Empty, options)
                ]);
        }

        public IBundleStorage GetStorage(IBundleOptions options)
        {
            return new TuanJieElementScanner(options.Package ?? string.Empty, options);
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.Package) 
                && options.Package.Contains("tuanjie"))
            {
                return true;
            }
            return false;
        }

        
    }
}
