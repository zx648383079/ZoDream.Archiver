using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class QuaternionCurveConverter : BundleConverter<QuaternionCurve>
    {
        public override QuaternionCurve? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                Curve = UnityConverter.ReadAnimationCurve(reader, reader.ReadVector4),
                Path = reader.ReadAlignedString()
            };
        }
    }

}
