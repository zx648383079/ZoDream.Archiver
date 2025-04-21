using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class MinMaxAABBConverter : BundleConverter<MinMaxAABB>
    {
        public override MinMaxAABB Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                Min = reader.ReadVector3Or4(),
                Max = reader.ReadVector3Or4(),
            };
        }
    }
}
