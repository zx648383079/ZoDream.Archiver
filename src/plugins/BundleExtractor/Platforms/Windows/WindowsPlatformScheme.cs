using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.BundleExtractor.Producers;

namespace ZoDream.BundleExtractor.Platforms
{
    public class WindowsPlatformScheme : IPlatformScheme
    {
        public string Root => throw new NotImplementedException();

        public IProducerScheme Producer => throw new NotImplementedException();

        public bool TryLoad(IEnumerable<string> fileItems)
        {
            throw new NotImplementedException();
        }
    }
}
