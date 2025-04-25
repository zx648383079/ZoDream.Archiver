using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class MovieTextureConverter : BundleConverter<MovieTexture>
    {
        public override MovieTexture? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new MovieTexture();
            UnityConverter.ReadTexture(res, reader, serializer);
            var m_Loop = reader.ReadBoolean();
            reader.AlignStream();
            res.AudioClip = reader.ReadPPtr<AudioClip>(serializer);

            res.MovieData = reader.ReadAsStream();
            return res;
        }

    }
}
