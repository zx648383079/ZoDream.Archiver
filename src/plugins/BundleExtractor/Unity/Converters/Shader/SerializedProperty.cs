using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SerializedPropertyConverter : BundleConverter<SerializedProperty>
    {
        public override SerializedProperty? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new SerializedProperty
            {
                Name = reader.ReadAlignedString(),
                Description = reader.ReadAlignedString(),
                Attributes = reader.ReadArray(r => r.ReadAlignedString()),
                Type = (SerializedPropertyType)reader.ReadInt32(),
                Flags = (SerializedPropertyFlag)reader.ReadUInt32(),
                DefValue = reader.ReadArray(4, (r, _) => r.ReadSingle()),
                DefTexture = serializer.Deserialize<SerializedTextureProperty>(reader)
            };
            return res;
        }
    }

}
