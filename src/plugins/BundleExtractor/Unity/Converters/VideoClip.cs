using System;
using System.IO;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{

    internal sealed class VideoClipConverter : BundleConverter<VideoClip>
    {
        public override VideoClip? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            var version = reader.Get<Version>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            var res = new VideoClip()
            {
                Name = reader.ReadAlignedString(),
                OriginalPath = reader.ReadAlignedString(),
            };
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
                var m_VideoShaders = reader.ReadPPtrArray<Shader>(serializer);
            }
            res.ExternalResources = new()
            {
                Source = reader.ReadAlignedString(),
                Offset = reader.ReadInt64(),
                Size = reader.ReadInt64()
            };
            var m_HasSplitAlpha = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(2020, 1)) //2020.1 and up
            {
                var m_sRGB = reader.ReadBoolean();
            }

            if (!string.IsNullOrEmpty(res.ExternalResources.Source))
            {
                var container = reader.Get<ISerializedFile>();
                if (reader.TryGet<IDependencyBuilder>(out var builder))
                {
                    var sourcePath = container.FullPath;
                    var fileId = reader.Get<ObjectInfo>().FileID;
                    builder.AddDependencyEntry(sourcePath, 
                        fileId, 
                        res.ExternalResources.Source);
                }
                res.VideoData = container.OpenResource(res.ExternalResources);
            }
            else
            {
                res.VideoData = new PartialStream(reader.BaseStream, res.ExternalResources.Size);
            }
            
            return res;
        }

        public void SaveAs(VideoClip data, string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, Path.GetExtension(data.OriginalPath), mode, out fileName))
            {
                return;
            }
            data.VideoData.SaveAs(fileName);
        }
    }
}
