using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class LeafInfoConstantConverter : BundleConverter<LeafInfoConstant>
    {
        public override LeafInfoConstant Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                IDArray = reader.ReadArray(r => r.ReadUInt32()),
                IndexOffset = reader.ReadUInt32()
            };
        }
    }
}
