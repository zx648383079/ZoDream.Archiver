using System.Collections.Generic;

namespace ZoDream.Shared.Interfaces
{
    public interface IBundleProducer
    {
        public bool TryLoad(IEnumerable<string> fileItems);
    }
}
