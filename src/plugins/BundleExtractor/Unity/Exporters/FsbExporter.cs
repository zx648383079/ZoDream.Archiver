using System.IO;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.FModExporter;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class FsbExporter(AudioClip audio) : IFileExporter
    {
        public string Name => audio.Name;
        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
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
            audio.m_AudioData.Position = 0;
            using var reader = new FModReader(audio.m_AudioData, Path.GetFileName(fileName), null);
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
            if (audio.IsOldVersion)
            {
                switch (audio.m_Type)
                {
                    case FMODSoundType.ACC:
                        return ".m4a";
                    case FMODSoundType.AIFF:
                        return ".aif";
                    case FMODSoundType.IT:
                        return ".it";
                    case FMODSoundType.MOD:
                        return ".mod";
                    case FMODSoundType.MPEG:
                        return ".mp3";
                    case FMODSoundType.OGGVORBIS:
                        return ".ogg";
                    case FMODSoundType.S3M:
                        return ".s3m";
                    case FMODSoundType.WAV:
                        return ".wav";
                    case FMODSoundType.XM:
                        return ".xm";
                    case FMODSoundType.XMA:
                        return ".wav";
                    case FMODSoundType.VAG:
                        return ".vag";
                    case FMODSoundType.AUDIOQUEUE:
                        return ".fsb";
                }

            }
            else
            {
                switch (audio.m_CompressionFormat)
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
                if (audio.IsOldVersion)
                {
                    return audio.m_Type switch
                    {
                        FMODSoundType.AIFF or FMODSoundType.IT or FMODSoundType.MOD or FMODSoundType.S3M or FMODSoundType.XM or FMODSoundType.XMA or FMODSoundType.AUDIOQUEUE => true,
                        _ => false,
                    };
                }
                else
                {
                    return audio.m_CompressionFormat switch
                    {
                        AudioCompressionFormat.PCM or AudioCompressionFormat.Vorbis or AudioCompressionFormat.ADPCM or AudioCompressionFormat.MP3 or AudioCompressionFormat.XMA => true,
                        _ => false,
                    };
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
