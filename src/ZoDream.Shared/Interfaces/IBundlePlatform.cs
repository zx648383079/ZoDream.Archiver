using System.Collections.Generic;

namespace ZoDream.Shared.Interfaces
{
    public interface IBundlePlatform
    {
        public string Root { get; }

        public IBundleProducer Producer { get; }
        public bool TryLoad(IEnumerable<string> fileItems);
    }
}
