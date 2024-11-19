using System.Collections.Generic;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleEngine: IBundleLoader
    {

        public IEnumerable<IBundleChunk> EnumerateChunk(IBundleSource fileItems, IBundleOptions options);

        public IBundleReader OpenRead(IBundleChunk fileItems, IBundleOptions options);
    }
}
