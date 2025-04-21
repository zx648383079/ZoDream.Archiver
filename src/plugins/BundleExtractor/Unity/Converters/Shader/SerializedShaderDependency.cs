using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SerializedShaderDependencyConverter : BundleConverter<SerializedShaderDependency>
    {
        public override SerializedShaderDependency Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                From = reader.ReadAlignedString(),
                To = reader.ReadAlignedString()
            };
        }
    }

}
