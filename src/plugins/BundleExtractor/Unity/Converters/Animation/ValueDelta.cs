using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class ValueDeltaConverter : BundleConverter<ValueDelta>
    {
        public override ValueDelta Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                Start = reader.ReadSingle(),
                Stop = reader.ReadSingle()
            };
        }
    }
}
