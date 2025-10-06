using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Producers
{
    public class HyperGryPhProducer : IBundleProducer
    {
        internal const string ProducerName = "HyperGryPh";

        public string AliasName => ProducerName;

        public IBundleSerializer CreateSerializer(IBundleOptions options)
        {
            return new BundleSerializer([
                .. UnityConverter.Converters,
                new HyperGryPhElementScanner(options.Package ?? string.Empty)
                ]);
        }

        public IBundleParser CreateParser(IBundleOptions options)
        {
            return new HyperGryPhElementScanner(options.Package ?? string.Empty);
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
