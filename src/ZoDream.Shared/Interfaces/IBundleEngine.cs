using System.Collections.Generic;

namespace ZoDream.Shared.Interfaces
{
    public interface IBundleEngine
    {
        
        public bool TryLoad(IEnumerable<string> fileItems);

        public IBundleReader OpenRead(IEnumerable<string> fileItems);
    }
}
