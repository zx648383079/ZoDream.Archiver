using System.Collections.Generic;

namespace ZoDream.Shared.Interfaces
{
    public interface IBundleProducer
    {
        public IEnumerable<IEnumerable<string>> EnumerateChunk();
        public bool TryLoad(IEnumerable<string> fileItems);
    }
}
