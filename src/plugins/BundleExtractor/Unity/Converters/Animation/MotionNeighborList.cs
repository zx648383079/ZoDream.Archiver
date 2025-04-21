using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class MotionNeighborListConverter : BundleConverter<MotionNeighborList>
    {
        public override MotionNeighborList Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                NeighborArray = reader.ReadArray(r => r.ReadUInt32())
            };
        }
    }
}
