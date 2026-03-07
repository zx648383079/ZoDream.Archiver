using System;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class SkinnedEffectConverter : BundleConverter<SkinnedEffect>
    {
        public override SkinnedEffect? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new SkinnedEffect()
            {
                Texture = XnbReader.ReadString(reader),
                WeightsPerVertex = reader.ReadInt32(),
                DiffuseColor = reader.ReadVector3(),
                EmissiveColor = reader.ReadVector3(),
                SpecularColor = reader.ReadVector3(),
                SpecularPower = reader.ReadSingle(),
                Alpha = reader.ReadSingle()
            };
        }
    }
}
