using System;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class SoundEffectConverter : BundleConverter<SoundEffect>
    {
        public override SoundEffect? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            Expectation.ThrowIfNot(reader.ReadInt32() == 18);
            return new SoundEffect()
            {

            };
        }
    }
}
