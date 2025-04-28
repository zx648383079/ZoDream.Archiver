using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SerializedPropertiesConverter : BundleConverter<SerializedProperties>
    {
        public override SerializedProperties? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new() 
            {
                Props = reader.ReadArray<SerializedProperty>(serializer)
            };
        }
    }

}
