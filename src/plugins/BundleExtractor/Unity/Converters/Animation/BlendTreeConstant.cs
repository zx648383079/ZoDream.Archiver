using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class BlendTreeConstantConverter : BundleConverter<BlendTreeConstant>
    {
        public override BlendTreeConstant? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new BlendTreeConstant
            {
                NodeArray = reader.ReadArray<BlendTreeNodeConstant>(serializer)
            };

            if (version.LessThan(4, 5)) //4.5 down
            {
                res.BlendEventArrayConstant = serializer.Deserialize<ValueArrayConstant>(reader);
            }
            return res;
        }
    }
}
