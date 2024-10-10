using System.Collections.Generic;

namespace ZoDream.Shared.Interfaces
{
    public interface IBundleScheme
    {
        public IBundleReader? Load(IEnumerable<string> fileItems, IArchiveOptions? options = null);

    }
}
