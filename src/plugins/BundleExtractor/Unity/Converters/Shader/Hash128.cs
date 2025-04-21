using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class Hash128Converter : BundleConverter<Hash128>
    {
        public override Hash128 Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new Hash128()
            {
                Value = reader.ReadBytes(16)
            };
        }
    }

}
