using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class ColliderConverter : BundleConverter<Collider>
    {
        public override Collider? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new Collider
            {
                X = reader.ReadXForm(),
                Type = reader.ReadUInt32(),
                XMotionType = reader.ReadUInt32(),
                YMotionType = reader.ReadUInt32(),
                ZMotionType = reader.ReadUInt32(),
                MinLimitX = reader.ReadSingle(),
                MaxLimitX = reader.ReadSingle(),
                MaxLimitY = reader.ReadSingle(),
                MaxLimitZ = reader.ReadSingle()
            };
            return res;
        }
    }

}
