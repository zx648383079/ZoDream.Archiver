using System.Collections.Generic;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleProducer
    {
        public bool TryLoad(IBundleSource fileItems, IBundleOptions options);
    }
}
