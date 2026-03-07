using System;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class EnvironmentMapEffectConverter : BundleConverter<EnvironmentMapEffect>
    {
        public override EnvironmentMapEffect? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new EnvironmentMapEffect()
            {
                Texture = XnbReader.ReadString(reader),
                EnvironmentMap = XnbReader.ReadString(reader),
                EnvironmentMapAmount = reader.ReadSingle(),
                EnvironmentMapSpecular = reader.ReadVector3(),
                FresnelFactor = reader.ReadSingle(),
                DiffuseColor = reader.ReadVector3(),
                EmissiveColor = reader.ReadVector3(),
                Alpha = reader.ReadSingle()
            };
        }
    }
}
