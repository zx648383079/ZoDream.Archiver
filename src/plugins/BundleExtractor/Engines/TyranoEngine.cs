using System.Collections.Generic;
using System.Linq;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Engines
{
    public class TyranoEngine : IBundleEngine
    {
        public const string EngineName = "Tyrano";
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
            if (fileItems.GetDirectories("tyrano").Any() || 
                fileItems.GetFiles("app.asar").Any())
            {
                options.Engine = EngineName;
                return true;
            }
            return false;
        }
    }
}
