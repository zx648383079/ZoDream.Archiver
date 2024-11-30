using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SerializedShaderFloatValue
    {
        public float val;
        public string name;

        public SerializedShaderFloatValue(UIReader reader)
        {
            val = reader.ReadSingle();
            name = reader.ReadAlignedString();
        }
    }

}
