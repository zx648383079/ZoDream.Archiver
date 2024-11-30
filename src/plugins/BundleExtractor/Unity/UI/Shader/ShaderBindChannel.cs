using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class ShaderBindChannel
    {
        public sbyte source;
        public sbyte target;

        public ShaderBindChannel(UIReader reader)
        {
            source = reader.ReadSByte();
            target = reader.ReadSByte();
        }
    }

}
