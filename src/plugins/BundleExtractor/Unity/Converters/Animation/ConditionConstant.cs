using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class ConditionConstantConverter : BundleConverter<ConditionConstant>
    {
        public override ConditionConstant Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new ConditionConstant
            {
                ConditionMode = reader.ReadUInt32(),
                EventID = reader.ReadUInt32(),
                EventThreshold = reader.ReadSingle(),
                ExitTime = reader.ReadSingle()
            };
            return res;
        }
    }
}
