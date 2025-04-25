using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class PaperProducer : IBundleProducer
    {
        internal const string ProducerName = "Paper";

        public string AliasName => ProducerName;

        public IBundleSerializer GetSerializer(IBundleOptions options)
        {
            return new BundleSerializer([
                .. Engines.UnityEngine.Converters,
                new PaperElementScanner(options.Package ?? string.Empty)
                ]);
        }

        public IBundleStorage GetStorage(IBundleOptions options)
        {
            return new PaperElementScanner(options.Package ?? string.Empty);
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }

        
    }
}
