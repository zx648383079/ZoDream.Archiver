using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SerializedTexturePropertyConverter : BundleConverter<SerializedTextureProperty>
    {
        public override SerializedTextureProperty Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                DefaultName = reader.ReadAlignedString(),
                TexDim = (TextureDimension)reader.ReadInt32()
            };
        }
    }

}
