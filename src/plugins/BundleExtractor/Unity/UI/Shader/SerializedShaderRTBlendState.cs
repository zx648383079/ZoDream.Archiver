using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SerializedShaderRTBlendState
    {
        public SerializedShaderFloatValue srcBlend;
        public SerializedShaderFloatValue targetBlend;
        public SerializedShaderFloatValue srcBlendAlpha;
        public SerializedShaderFloatValue targetBlendAlpha;
        public SerializedShaderFloatValue blendOp;
        public SerializedShaderFloatValue blendOpAlpha;
        public SerializedShaderFloatValue colMask;

        public SerializedShaderRTBlendState(UIReader reader)
        {
            srcBlend = new SerializedShaderFloatValue(reader);
            targetBlend = new SerializedShaderFloatValue(reader);
            srcBlendAlpha = new SerializedShaderFloatValue(reader);
            targetBlendAlpha = new SerializedShaderFloatValue(reader);
            blendOp = new SerializedShaderFloatValue(reader);
            blendOpAlpha = new SerializedShaderFloatValue(reader);
            colMask = new SerializedShaderFloatValue(reader);
        }
    }

}
