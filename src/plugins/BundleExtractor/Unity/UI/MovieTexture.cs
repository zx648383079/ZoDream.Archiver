﻿using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class MovieTexture(UIReader reader) : Texture(reader), IFileExporter
    {
        public Stream MovieData;
        public PPtr<AudioClip> m_AudioClip;

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            var m_Loop = reader.ReadBoolean();
            reader.AlignStream();
            m_AudioClip = new PPtr<AudioClip>(reader);

            MovieData = reader.ReadAsStream();
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".ogv", mode, out fileName))
            {
                return;
            }
            MovieData.SaveAs(fileName);
        }
    }
}
