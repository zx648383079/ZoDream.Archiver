using System.Collections.Generic;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Engines
{
    public class UnknownEngine : IBundleEngine
    {
        public IBundleReader OpenRead(IEnumerable<string> fileItems)
        {
            return null;
        }

        public bool TryLoad(IEnumerable<string> fileItems)
        {
            return true;
        }
    }
}
