using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor
{
    public class UnityBundleReader : IBundleReader
    {
        public void ExtractTo(string folder, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}
