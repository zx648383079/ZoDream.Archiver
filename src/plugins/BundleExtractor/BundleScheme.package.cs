using ZoDream.Shared.Interfaces;
using System.Collections.Generic;
using ZoDream.BundleExtractor.Producers;
using ZoDream.BundleExtractor.Platforms;
using ZoDream.BundleExtractor.Engines;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme
    {
        public IBundlePlatform? TryGet(string packeageName)
        {
            if (packeageName.EndsWith(".qjnn"))
            {
                return new AndroidPlatformScheme(new PaperProducer(), 
                    new CocosEngine());
            }
            if (packeageName.StartsWith("com.papegames."))
            {
                return new AndroidPlatformScheme(new PaperProducer(), new UnityEngine());
            }
            return null;
        }

        internal static bool TryGet(
            string packeageName, 
            out IBundleProducer producer,
            out IBundleEngine engine)
        {
            if (packeageName.EndsWith(".qjnn"))
            {
                producer = new PaperProducer();
                engine = new CocosEngine();
                return true;
            }
            if (packeageName.StartsWith("com.papegames."))
            {
                producer = new PaperProducer();
                engine = new UnityEngine();
                return true;
            }
            producer = null;
            engine = null;
            return false;
        }
    }
}
