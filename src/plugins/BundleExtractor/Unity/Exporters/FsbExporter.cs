using System.IO;
using UnityEngine;
using ZoDream.FModExporter;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class FsbExporter(int entryId, ISerializedFile resource) : IBundleExporter
    {
        public string FileName => resource[entryId].Name;
        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (resource[entryId] is not AudioClip audio)
            {
                return;
            }
            //audio.m_AudioData.Position = 0;
            //if (FsbLoader.TryLoadFsbFromByteArray(audio.m_AudioData.ToArray(), out var instance))
            //{
            //    if (instance!.Samples[0].RebuildAsStandardFileFormat(out var buffer, out var ext) 
            //        && LocationStorage.TryCreate(fileName, $".{ext}", mode, out fileName))
            //    {
            //        File.WriteAllBytes(fileName, buffer);
            //    }
            //    return;
            //}
            //if (!LocationStorage.TryCreate(fileName, ".wav", mode, out fileName))
            //{
            //    return;
            //}
            audio.AudioData.Position = 0;
            using var reader = new FModReader(audio.AudioData, Path.GetFileName(fileName), null);
            reader.ExtractToDirectory(Path.GetDirectoryName(fileName), mode);
            // using var fs = File.Create(fileName);
            //Audio.Decode(audio.m_AudioData, m_CompressionFormat switch
            //{
            //    AudioCompressionFormat.PCM => AVCodecID.AV_CODEC_ID_ADPCM_4XM,
            //    AudioCompressionFormat.Vorbis => AVCodecID.AV_CODEC_ID_VORBIS,
            //    AudioCompressionFormat.ADPCM => AVCodecID.AV_CODEC_ID_ADPCM_4XM,
            //    AudioCompressionFormat.MP3 => AVCodecID.AV_CODEC_ID_MP3,
            //    AudioCompressionFormat.PSMVAG => AVCodecID.AV_CODEC_ID_XMA2,
            //    AudioCompressionFormat.HEVAG => AVCodecID.AV_CODEC_ID_XMA2,
            //    AudioCompressionFormat.XMA => AVCodecID.AV_CODEC_ID_XMA2,
            //    AudioCompressionFormat.AAC => AVCodecID.AV_CODEC_ID_AAC,
            //    AudioCompressionFormat.GCADPCM => AVCodecID.AV_CODEC_ID_XMA2,
            //    AudioCompressionFormat.ATRAC9 => AVCodecID.AV_CODEC_ID_XMA2,
            //    _ => AVCodecID.AV_CODEC_ID_XMA2,
            //}, fs);
        }

        public string GetExtensionName()
        {
            if (resource[entryId] is not AudioClip audio)
            {
                return ".AudioClip";
            }
            if (resource.Version.LessThan(5))
            {
                switch (audio.Type)
                {
                    case AudioType.ACC:
                        return ".m4a";
                    case AudioType.AIFF:
                        return ".aif";
                    case AudioType.IT:
                        return ".it";
                    case AudioType.MOD:
                        return ".mod";
                    case AudioType.MPEG:
                        return ".mp3";
                    case AudioType.OGGVORBIS:
                        return ".ogg";
                    case AudioType.S3M:
                        return ".s3m";
                    case AudioType.WAV:
                        return ".wav";
                    case AudioType.XM:
                        return ".xm";
                    case AudioType.XMA:
                        return ".wav";
                    case AudioType.VAG:
                        return ".vag";
                    case AudioType.AUDIOQUEUE:
                        return ".fsb";
                }

            }
            else
            {
                switch (audio.CompressionFormat)
                {
                    case AudioCompressionFormat.PCM:
                        return ".fsb";
                    case AudioCompressionFormat.Vorbis:
                        return ".fsb";
                    case AudioCompressionFormat.ADPCM:
                        return ".fsb";
                    case AudioCompressionFormat.MP3:
                        return ".fsb";
                    case AudioCompressionFormat.PSMVAG:
                        return ".fsb";
                    case AudioCompressionFormat.HEVAG:
                        return ".fsb";
                    case AudioCompressionFormat.XMA:
                        return ".fsb";
                    case AudioCompressionFormat.AAC:
                        return ".m4a";
                    case AudioCompressionFormat.GCADPCM:
                        return ".fsb";
                    case AudioCompressionFormat.ATRAC9:
                        return ".fsb";
                }
            }

            return ".AudioClip";
        }

        public bool IsSupport {
            get {
                if (resource[entryId] is not AudioClip audio)
                {
                    return false;
                }
                if (resource.Version.LessThan(5))
                {
                    return audio.Type switch
                    {
                        AudioType.AIFF or AudioType.IT or AudioType.MOD or AudioType.S3M or AudioType.XM or AudioType.XMA or AudioType.AUDIOQUEUE => true,
                        _ => false,
                    };
                }
                else
                {
                    return audio.CompressionFormat switch
                    {
                        AudioCompressionFormat.PCM or AudioCompressionFormat.Vorbis or AudioCompressionFormat.ADPCM or AudioCompressionFormat.MP3 or AudioCompressionFormat.XMA => true,
                        _ => false,
                    };
                }
            }
        }
    }
}
