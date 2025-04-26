using System;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class AudioClipConverter : BundleConverter<AudioClip>
    {
        public override AudioClip? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            var version = reader.Get<Version>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            var res = new AudioClip
            {
                Name = reader.ReadAlignedString()
            };
            if (version.LessThan(5))
            {
                res.Format = reader.ReadInt32();
                res.Type = (AudioType)reader.ReadInt32();
                res.Is3D = reader.ReadBoolean();
                res.UseHardware = reader.ReadBoolean();
                reader.AlignStream();

                if (version.GreaterThanOrEquals(3, 2)) //3.2.0 to 5
                {
                    int m_Stream = reader.ReadInt32();
                    res.Size = reader.ReadInt32();
                    var tsize = res.Size % 4 != 0 ? res.Size + 4 - res.Size % 4 : res.Size;
                    if (reader.RemainingLength != tsize)
                    {
                        res.Offset = reader.ReadUInt32();
                        res.Source = reader.Get<ISerializedFile>().FullPath + ".resS";
                    }
                }
                else
                {
                    res.Size = reader.ReadInt32();
                }
            }
            else
            {
                res.LoadType = reader.ReadInt32();
                res.Channels = reader.ReadInt32();
                res.Frequency = reader.ReadInt32();
                res.BitsPerSample = reader.ReadInt32();
                res.Length = reader.ReadSingle();
                res.IsTrackerFormat = reader.ReadBoolean();
                reader.AlignStream();
                res.SubSoundIndex = reader.ReadInt32();
                res.PreloadAudioData = reader.ReadBoolean();
                res.LoadInBackground = reader.ReadBoolean();
                res.Legacy3D = reader.ReadBoolean();
                reader.AlignStream();

                //StreamedResource m_Resource
                res.Source = reader.ReadAlignedString();
                res.Offset = reader.ReadInt64();
                res.Size = reader.ReadInt64();
                res.CompressionFormat = (AudioCompressionFormat)reader.ReadInt32();
            }

            if (!string.IsNullOrEmpty(res.Source))
            {
                var container = reader.Get<ISerializedFile>();
                if (reader.TryGet<IDependencyBuilder>(out var builder))
                {
                    var fileName = container.FullPath;
                    var fileId = reader.Get<ObjectInfo>().FileID;
                    builder.AddDependencyEntry(fileName,
                        fileId,
                        res.Source);
                }
                res.AudioData = container.OpenResource(new()
                {
                    Source = res.Source, 
                    Offset = res.Offset, 
                    Size = res.Size
                });
            } else
            {
                res.AudioData = new PartialStream(reader.BaseStream, res.Size);
            }
            return res;
        }

    }

}
