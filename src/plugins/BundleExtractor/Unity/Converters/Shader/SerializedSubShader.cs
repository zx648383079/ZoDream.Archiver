using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SerializedSubShaderConverter : BundleConverter<SerializedSubShader>
    {
        public override SerializedSubShader? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                Passes = reader.ReadArray(_ => serializer.Deserialize<SerializedPass>(reader)),
                Tags = serializer.Deserialize<SerializedTagMap>(reader),
                LOD = reader.ReadInt32()
            };
        }
    }

}
