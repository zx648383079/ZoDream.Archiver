using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class CompressedAnimationCurveConverter : BundleConverter<CompressedAnimationCurve>
    {
        public override CompressedAnimationCurve? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new CompressedAnimationCurve
            {
                Path = reader.ReadAlignedString(),
                Times = serializer.Deserialize<PackedIntVector>(reader),
                Values = serializer.Deserialize<PackedQuatVector>(reader),
                Slopes = serializer.Deserialize<PackedFloatVector>(reader),
                PreInfinity = reader.ReadInt32(),
                PostInfinity = reader.ReadInt32()
            };
            return res;
        }

    }

}
