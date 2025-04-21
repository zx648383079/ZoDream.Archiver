using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SamplerParameterConverter : BundleConverter<SamplerParameter>
    {
        public override SamplerParameter Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                Sampler = reader.ReadUInt32(),
                BindPoint = reader.ReadInt32()
            };
        }
    }
}
