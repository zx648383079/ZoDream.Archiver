using System;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class DualTextureEffectConverter : BundleConverter<DualTextureEffect>
    {
        public override DualTextureEffect? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new DualTextureEffect()
            {
                Texture = XnbReader.ReadString(reader),
                Texture2 = XnbReader.ReadString(reader),
                DiffuseColor = reader.ReadVector3(),
                Alpha = reader.ReadSingle(),
                VertexColorEnabled = reader.ReadBoolean()
            };
        }
    }
}
