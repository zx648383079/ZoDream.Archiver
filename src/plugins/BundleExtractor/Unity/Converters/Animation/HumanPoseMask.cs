using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class HumanPoseMaskConverter : BundleConverter<HumanPoseMask>
    {
        public override HumanPoseMask Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new HumanPoseMask
            {
                Word0 = reader.ReadUInt32(),
                Word1 = reader.ReadUInt32()
            };
            if (version.GreaterThanOrEquals(5, 2)) //5.2 and up
            {
                res.Word2 = reader.ReadUInt32();
            }
            return res;
        }
    }
}
