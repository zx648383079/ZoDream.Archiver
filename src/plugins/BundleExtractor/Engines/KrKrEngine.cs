using System.Collections.Generic;
using System.Linq;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Engines
{
    public class KrKrEngine : IBundleEngine
    {
        public const string EngineName = "krkr";
        public IEnumerable<IBundleChunk> EnumerateChunk(IBundleSource fileItems)
        {
            foreach (var item in fileItems)
            {
                yield return new BundleChunk(item);
            }
        }

        public IBundleReader OpenRead(IBundleChunk fileItems, IBundleOptions options)
        {
            return null;
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            if (fileItems.GetFiles("*.xp3").Any())
            {
                options.Engine = EngineName;
                return true;
            }
            return false;
        }
    }
}
