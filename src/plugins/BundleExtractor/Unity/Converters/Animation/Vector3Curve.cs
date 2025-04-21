using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class Vector3CurveConverter : BundleConverter<Vector3Curve>
    {
        public override Vector3Curve? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new Vector3Curve
            {
                Curve = UnityConverter.ReadAnimationCurve(reader, reader.ReadVector3Or4),
                Path = reader.ReadAlignedString()
            };

        }

    }

}
