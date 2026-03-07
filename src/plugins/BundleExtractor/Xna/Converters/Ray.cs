using System;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class RayConverter : BundleConverter<Ray>
    {
        public override Ray Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new Ray()
            {
                Position = reader.ReadVector3(),
                Direction = reader.ReadVector3(),
            };
        }
    }
}
