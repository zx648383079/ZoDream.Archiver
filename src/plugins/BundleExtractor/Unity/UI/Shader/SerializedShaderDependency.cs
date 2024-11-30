using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SerializedShaderDependency
    {
        public string from;
        public string to;

        public SerializedShaderDependency(UIReader reader)
        {
            from = reader.ReadAlignedString();
            to = reader.ReadAlignedString();
        }
    }

}
