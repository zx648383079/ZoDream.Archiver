using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class ShaderBindChannelConverter : BundleConverter<ShaderBindChannel>
    {
        public override ShaderBindChannel Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new(){
                Source = reader.ReadSByte(),
                Target = reader.ReadSByte(),
            };
        }
    }

}
