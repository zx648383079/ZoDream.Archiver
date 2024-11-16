using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Engines
{
    public class UnrealEngine : IBundleEngine
    {
        public const string EngineName = "Unreal";
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
            return false;
        }
    }
}
