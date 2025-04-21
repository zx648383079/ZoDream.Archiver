using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class ValueArrayConstantConverter : BundleConverter<ValueArrayConstant>
    {
        public override ValueArrayConstant? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                ValueArray = reader.ReadArray<ValueConstant>(serializer)
            };
        }
    }
}
