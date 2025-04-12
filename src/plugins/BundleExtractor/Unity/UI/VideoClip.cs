using System.Collections.Generic;
using System.IO;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class StreamedResource
    {
        public string m_Source;
        public long m_Offset; //ulong
        public long m_Size; //ulong

        public StreamedResource(IBundleBinaryReader reader)
        {
            m_Source = reader.ReadAlignedString();
            m_Offset = reader.ReadInt64();
            m_Size = reader.ReadInt64();
        }
    }

    internal sealed class VideoClip(UIReader reader) : NamedObject(reader), IFileExporter
    {
        public Stream m_VideoData;
        public string m_OriginalPath;
        public StreamedResource m_ExternalResources;

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            var version = reader.Get<UnityVersion>();
            m_OriginalPath = reader.ReadAlignedString();
            var m_ProxyWidth = reader.ReadUInt32();
            var m_ProxyHeight = reader.ReadUInt32();
            var Width = reader.ReadUInt32();
            var Height = reader.ReadUInt32();
            if (version.GreaterThanOrEquals(2017, 2)) //2017.2 and up
            {
                var m_PixelAspecRatioNum = reader.ReadUInt32();
                var m_PixelAspecRatioDen = reader.ReadUInt32();
            }
            var m_FrameRate = reader.ReadDouble();
            var m_FrameCount = reader.ReadUInt64();
            var m_Format = reader.ReadInt32();
            var m_AudioChannelCount = reader.ReadArray(r => r.ReadUInt16());
            reader.AlignStream();
            var m_AudioSampleRate = reader.ReadArray(r => r.ReadUInt32());
            var m_AudioLanguage = reader.ReadArray(r => r.ReadAlignedString());
            if (version.GreaterThanOrEquals(2020, 1)) //2020.1 and up
            {
                var m_VideoShadersSize = reader.ReadInt32();
                var m_VideoShaders = new List<PPtr<Shader>>();
                for (int i = 0; i < m_VideoShadersSize; i++)
                {
                    m_VideoShaders.Add(new PPtr<Shader>(reader));
                }
            }
            m_ExternalResources = new StreamedResource(reader);
            var m_HasSplitAlpha = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(2020, 1)) //2020.1 and up
            {
                var m_sRGB = reader.ReadBoolean();
            }

            if (!string.IsNullOrEmpty(m_ExternalResources.m_Source))
            {
                m_VideoData = _reader.OpenResource(m_ExternalResources.m_Source, m_ExternalResources.m_Offset, m_ExternalResources.m_Size);
            }
            else
            {
                m_VideoData = new PartialStream(reader.BaseStream, m_ExternalResources.m_Size);
            }
        }

        public override void Associated(IDependencyBuilder? builder)
        {
            base.Associated(builder);
            if (!string.IsNullOrEmpty(m_ExternalResources.m_Source))
            {
                builder?.AddDependencyEntry(_reader.FullPath, FileID, m_ExternalResources.m_Source);
            }
        }
        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, Path.GetExtension(m_OriginalPath), mode, out fileName))
            {
                return;
            }
            m_VideoData.SaveAs(fileName);
        }
    }
}
