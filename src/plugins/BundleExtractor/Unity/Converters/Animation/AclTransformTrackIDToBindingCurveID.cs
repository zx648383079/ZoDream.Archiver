using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class AclTransformTrackIDToBindingCurveIDConverter : BundleConverter<AclTransformTrackIDToBindingCurveID>
    {
        public override AclTransformTrackIDToBindingCurveID Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                RotationIDToBindingCurveID = reader.ReadUInt32(),
                PositionIDToBindingCurveID = reader.ReadUInt32(),
                ScaleIDToBindingCurveID = reader.ReadUInt32()
            };
        }
    }

}
