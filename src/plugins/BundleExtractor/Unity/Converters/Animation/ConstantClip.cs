using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class ConstantClipConverter : BundleConverter<ConstantClip>
    {
        public override ConstantClip Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                Data = reader.ReadArray(r => r.ReadSingle())
            };
        }
    }
}
