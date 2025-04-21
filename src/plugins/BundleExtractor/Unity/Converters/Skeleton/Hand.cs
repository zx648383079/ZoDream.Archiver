using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class HandConverter : BundleConverter<Hand>
    {
        public override Hand Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                HandBoneIndex = reader.ReadArray(r => r.ReadInt32())
            };
        }
    }

}
