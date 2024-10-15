using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.BundleExtractor.Producers;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Platforms
{
    public class IosPlatformScheme : IBundlePlatform
    {
        public string Root => throw new NotImplementedException();

        public IBundleProducer Producer => throw new NotImplementedException();

        public IBundleEngine Engine => throw new NotImplementedException();

        public bool TryLoad(IEnumerable<string> fileItems)
        {
            return false;
        }
    }
}
