using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor
{
    public class UnityBundleScheme : IBundleScheme
    {
        public IBundleReader? Load(string folder, IArchiveOptions? options = null)
        {
            throw new NotImplementedException();
        }
    }
}
