using System;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class EffectConverter : BundleConverter<Effect>
    {
        public override Effect? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new Effect()
            {
                Data = reader.ReadAsStream()
            };
        }
    }
}
