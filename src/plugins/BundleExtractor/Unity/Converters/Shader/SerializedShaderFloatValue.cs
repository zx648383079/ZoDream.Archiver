using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SerializedShaderFloatValueConverter : BundleConverter<SerializedShaderFloatValue>
    {
        public override SerializedShaderFloatValue Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                Value = reader.ReadSingle(),
                Name = reader.ReadAlignedString(),
            };
        }
    }

}
