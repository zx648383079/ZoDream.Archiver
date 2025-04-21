using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SerializedShaderRTBlendStateConverter : BundleConverter<SerializedShaderRTBlendState>
    {
        public override SerializedShaderRTBlendState Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new SerializedShaderRTBlendState
            {
                SrcBlend = serializer.Deserialize<SerializedShaderFloatValue>(reader),
                TargetBlend = serializer.Deserialize<SerializedShaderFloatValue>(reader),
                SrcBlendAlpha = serializer.Deserialize<SerializedShaderFloatValue>(reader),
                TargetBlendAlpha = serializer.Deserialize<SerializedShaderFloatValue>(reader),
                BlendOp = serializer.Deserialize<SerializedShaderFloatValue>(reader),
                BlendOpAlpha = serializer.Deserialize<SerializedShaderFloatValue>(reader),
                ColMask = serializer.Deserialize<SerializedShaderFloatValue>(reader)
            };
            return res;
        }
    }

}
