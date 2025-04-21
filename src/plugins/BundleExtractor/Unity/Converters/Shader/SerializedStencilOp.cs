using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SerializedStencilOpConverter : BundleConverter<SerializedStencilOp>
    {
        public override SerializedStencilOp Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new SerializedStencilOp
            {
                Pass = serializer.Deserialize<SerializedShaderFloatValue>(reader),
                Fail = serializer.Deserialize<SerializedShaderFloatValue>(reader),
                ZFail = serializer.Deserialize<SerializedShaderFloatValue>(reader),
                Comp = serializer.Deserialize<SerializedShaderFloatValue>(reader)
            };
            return res;
        }
    }

}
