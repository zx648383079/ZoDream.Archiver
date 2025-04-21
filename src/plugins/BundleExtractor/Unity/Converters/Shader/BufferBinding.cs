using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class BufferBindingConverter : BundleConverter<BufferBinding>
    {
        public override BufferBinding Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new BufferBinding();
            res.NameIndex = reader.ReadInt32();
            res.Index = reader.ReadInt32();
            if (version.GreaterThanOrEquals(2020, 1)) //2020.1 and up
            {
                res.ArraySize = reader.ReadInt32();
            }
            return res;
        }
    }

}
