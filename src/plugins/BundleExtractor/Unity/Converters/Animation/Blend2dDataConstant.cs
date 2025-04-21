using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class Blend2dDataConstantConverter : BundleConverter<Blend2dDataConstant>
    {
        public override Blend2dDataConstant? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new Blend2dDataConstant
            {
                ChildPositionArray = reader.ReadArray(_ => reader.ReadVector2()),
                ChildMagnitudeArray = reader.ReadArray(r => r.ReadSingle()),
                ChildPairVectorArray = reader.ReadArray(_ => reader.ReadVector2()),
                ChildPairAvgMagInvArray = reader.ReadArray(r => r.ReadSingle()),

                ChildNeighborListArray = reader.ReadArray<MotionNeighborList>(serializer)
            };
            return res;
        }
    }
}
