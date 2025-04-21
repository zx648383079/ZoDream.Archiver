using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SelectorTransitionConstantConverter : BundleConverter<SelectorTransitionConstant>
    {
        public override SelectorTransitionConstant? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                Destination = reader.ReadUInt32(),
                ConditionConstantArray = reader.ReadArray<ConditionConstant>(serializer)
            };
        }
    }
}
