using System.Collections.Generic;

namespace ZoDream.Shared.Interfaces
{
    public interface IBundleEngine
    {

        public IEnumerable<IBundleChunk> EnumerateChunk();

        public bool TryLoad(IEnumerable<string> fileItems);

        public IBundleReader OpenRead(IBundleChunk fileItems);
    }
}
