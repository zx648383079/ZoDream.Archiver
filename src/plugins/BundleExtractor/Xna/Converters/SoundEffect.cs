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
            var count = reader.ReadInt32();
            Expectation.ThrowIfNot(count == 18);
            return new SoundEffect()
            {
                Header = reader.ReadBytes(count),
                Data = reader.ReadAsStream(),
                LoopStart = reader.ReadInt32(),
                LoopLength = reader.ReadInt32(),
                Duration = reader.ReadInt32(),// ms
            };
        }
    }
}
