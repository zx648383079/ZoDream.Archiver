using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class MeshBlendShapeChannelConverter : BundleConverter<MeshBlendShapeChannel>
    {
        public override MeshBlendShapeChannel Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                Name = reader.ReadAlignedString(),
                NameHash = reader.ReadUInt32(),
                FrameIndex = reader.ReadInt32(),
                FrameCount = reader.ReadInt32()
            };
        }
    }

}
