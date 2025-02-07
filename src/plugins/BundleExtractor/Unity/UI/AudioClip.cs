using FFmpeg.AutoGen;
using Fmod5Sharp;
using System.IO;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Media;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class AudioClip(UIReader reader) : NamedObject(reader), IFileExporter
    {
        public int m_Format;
        public FMODSoundType m_Type;
        public bool m_3D;
        public bool m_UseHardware;

        //version 5
        public int m_LoadType;
        public int m_Channels;
        public int m_Frequency;
        public int m_BitsPerSample;
        public float m_Length;
        public bool m_IsTrackerFormat;
        public int m_SubsoundIndex;
        public bool m_PreloadAudioData;
        public bool m_LoadInBackground;
        public bool m_Legacy3D;
        public AudioCompressionFormat m_CompressionFormat;

        public string m_Source;
        public long m_Offset; //ulong
        public long m_Size; //ulong
        public Stream m_AudioData;

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            var version = reader.Get<UnityVersion>();
            if (version.LessThan(5))
            {
                m_Format = reader.ReadInt32();
                m_Type = (FMODSoundType)reader.ReadInt32();
                m_3D = reader.ReadBoolean();
                m_UseHardware = reader.ReadBoolean();
                reader.AlignStream();

                if (version.GreaterThanOrEquals(3, 2)) //3.2.0 to 5
                {
                    int m_Stream = reader.ReadInt32();
                    m_Size = reader.ReadInt32();
                    var tsize = m_Size % 4 != 0 ? m_Size + 4 - m_Size % 4 : m_Size;
                    if (_reader.Data.ByteSize + _reader.Data.ByteStart - reader.Position != tsize)
                    {
                        m_Offset = reader.ReadUInt32();
                        m_Source = _reader.FullPath + ".resS";
                    }
                }
                else
                {
                    m_Size = reader.ReadInt32();
                }
            }
            else
            {
                m_LoadType = reader.ReadInt32();
                m_Channels = reader.ReadInt32();
                m_Frequency = reader.ReadInt32();
                m_BitsPerSample = reader.ReadInt32();
                m_Length = reader.ReadSingle();
                m_IsTrackerFormat = reader.ReadBoolean();
                reader.AlignStream();
                m_SubsoundIndex = reader.ReadInt32();
                m_PreloadAudioData = reader.ReadBoolean();
                m_LoadInBackground = reader.ReadBoolean();
                m_Legacy3D = reader.ReadBoolean();
                reader.AlignStream();

                //StreamedResource m_Resource
                m_Source = reader.ReadAlignedString();
                m_Offset = reader.ReadInt64();
                m_Size = reader.ReadInt64();
                m_CompressionFormat = (AudioCompressionFormat)reader.ReadInt32();
            }

            if (!string.IsNullOrEmpty(m_Source))
            {
                m_AudioData = new PartialStream(_reader.OpenResource(m_Source), m_Offset, m_Size);
            } else
            {
                m_AudioData = new PartialStream(reader.BaseStream, m_Size);
            }
            
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            m_AudioData.Position = 0;
            if (FsbLoader.TryLoadFsbFromByteArray(m_AudioData.ToArray(), out var instance))
            {
                if (instance!.Samples[0].RebuildAsStandardFileFormat(out var buffer, out var ext) 
                    && LocationStorage.TryCreate(fileName, $".{ext}", mode, out fileName))
                {
                    File.WriteAllBytes(fileName, buffer);
                }
                return;
            }
            if (!LocationStorage.TryCreate(fileName, ".wav", mode, out fileName))
            {
                return;
            }
            m_AudioData.Position = 0;
            using var fs = File.Create(fileName);
            Audio.Decode(m_AudioData, m_CompressionFormat switch
            {
                AudioCompressionFormat.PCM => AVCodecID.AV_CODEC_ID_ADPCM_4XM,
                AudioCompressionFormat.Vorbis => AVCodecID.AV_CODEC_ID_VORBIS,
                AudioCompressionFormat.ADPCM => AVCodecID.AV_CODEC_ID_ADPCM_4XM,
                AudioCompressionFormat.MP3 => AVCodecID.AV_CODEC_ID_MP3,
                AudioCompressionFormat.PSMVAG => AVCodecID.AV_CODEC_ID_XMA2,
                AudioCompressionFormat.HEVAG => AVCodecID.AV_CODEC_ID_XMA2,
                AudioCompressionFormat.XMA => AVCodecID.AV_CODEC_ID_XMA2,
                AudioCompressionFormat.AAC => AVCodecID.AV_CODEC_ID_AAC,
                AudioCompressionFormat.GCADPCM => AVCodecID.AV_CODEC_ID_XMA2,
                AudioCompressionFormat.ATRAC9 => AVCodecID.AV_CODEC_ID_XMA2,
                _ => AVCodecID.AV_CODEC_ID_XMA2,
            }, fs);
        }
    }

}
