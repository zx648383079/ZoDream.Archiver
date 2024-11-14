using System.Collections.Generic;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleEngine
    {

        public IEnumerable<IBundleChunk> EnumerateChunk(IBundleSource fileItems);

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options);

        public IBundleReader OpenRead(IBundleChunk fileItems, IBundleOptions options);
    }
}
