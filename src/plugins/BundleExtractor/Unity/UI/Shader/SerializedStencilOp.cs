using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SerializedStencilOp
    {
        public SerializedShaderFloatValue pass;
        public SerializedShaderFloatValue fail;
        public SerializedShaderFloatValue zFail;
        public SerializedShaderFloatValue comp;

        public SerializedStencilOp(UIReader reader)
        {
            pass = new SerializedShaderFloatValue(reader);
            fail = new SerializedShaderFloatValue(reader);
            zFail = new SerializedShaderFloatValue(reader);
            comp = new SerializedShaderFloatValue(reader);
        }
    }

}
