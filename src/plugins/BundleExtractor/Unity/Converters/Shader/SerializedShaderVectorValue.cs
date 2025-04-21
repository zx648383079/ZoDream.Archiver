using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SerializedShaderVectorValueConverter : BundleConverter<SerializedShaderVectorValue>
    {
        public override SerializedShaderVectorValue Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new SerializedShaderVectorValue
            {
                X = serializer.Deserialize<SerializedShaderFloatValue>(reader),
                Y = serializer.Deserialize<SerializedShaderFloatValue>(reader),
                Z = serializer.Deserialize<SerializedShaderFloatValue>(reader),
                W = serializer.Deserialize<SerializedShaderFloatValue>(reader),
                Name = reader.ReadAlignedString()
            };
            return res;
        }
    }

}
