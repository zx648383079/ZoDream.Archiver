using System.Collections;
using System.Collections.Generic;

namespace ZoDream.BundleExtractor.Producers
{
    public interface IProducerScheme
    {
        public IEnumerable<IEnumerable<string>> EnumerateChunk();
        public bool TryLoad(IEnumerable<string> fileItems);
    }
}
