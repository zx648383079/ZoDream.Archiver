using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Hash128
    {
        public byte[] bytes;

        public Hash128(UIReader reader)
        {
            bytes = reader.ReadBytes(16);
        }
    }

}
