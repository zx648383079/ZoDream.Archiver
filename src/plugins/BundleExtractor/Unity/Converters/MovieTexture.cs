using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class MovieTextureConverter : BundleConverter<MovieTexture>, IBundleExporter
    {
        public override MovieTexture? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new MovieTexture();
            UnityConverter.ReadTexture(res, reader, serializer);
            var m_Loop = reader.ReadBoolean();
            reader.AlignStream();
            res.AudioClip = serializer.Deserialize<PPtr<AudioClip>>(reader);

            res.MovieData = reader.ReadAsStream();
            return res;
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".ogv", mode, out fileName))
            {
                return;
            }
            res.MovieData.SaveAs(fileName);
        }
    }
}
