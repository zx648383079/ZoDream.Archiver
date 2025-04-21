using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class AABBConverter : BundleConverter<Bounds>
    {
        public override Bounds Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                Center = reader.ReadVector3Or4(),
                Extent = reader.ReadVector3Or4()
            };
        }

    }

}
