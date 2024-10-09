using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Producers
{
    public class PaperProducer : IProducerScheme
    {
        public IEnumerable<IEnumerable<string>> EnumerateChunk()
        {
            throw new NotImplementedException();
        }

        public bool TryLoad(IEnumerable<string> fileItems)
        {
            throw new NotImplementedException();
        }
    }
}
