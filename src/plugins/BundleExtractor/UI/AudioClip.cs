using System.IO;
using ZoDream.Shared.IO;
using ZoDream.Shared.Media;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.UI
{
    public sealed class AudioClip : NamedObject, IFileWriter
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

        public AudioClip(UIReader reader) : base(reader)
        {
            if (reader.Version.LessThan(5))
            {
                var version = reader.Version;
                m_Format = reader.Reader.ReadInt32();
                m_Type = (FMODSoundType)reader.Reader.ReadInt32();
                m_3D = reader.Reader.ReadBoolean();
                m_UseHardware = reader.Reader.ReadBoolean();
                reader.Reader.AlignStream();

                if (version.GreaterThanOrEquals(3,2)) //3.2.0 to 5
                {
                    int m_Stream = reader.Reader.ReadInt32();
                    m_Size = reader.Reader.ReadInt32();
                    var tsize = m_Size % 4 != 0 ? m_Size + 4 - m_Size % 4 : m_Size;
                    if (reader.Data.ByteSize + reader.Data.ByteStart - reader.Position != tsize)
                    {
                        m_Offset = reader.Reader.ReadUInt32();
                        m_Source = reader.FullPath + ".resS";
                    }
                }
                else
                {
                    m_Size = reader.Reader.ReadInt32();
                }
            }
            else
            {
                m_LoadType = reader.Reader.ReadInt32();
                m_Channels = reader.Reader.ReadInt32();
                m_Frequency = reader.Reader.ReadInt32();
                m_BitsPerSample = reader.Reader.ReadInt32();
                m_Length = reader.Reader.ReadSingle();
                m_IsTrackerFormat = reader.Reader.ReadBoolean();
                reader.Reader.AlignStream();
                m_SubsoundIndex = reader.Reader.ReadInt32();
                m_PreloadAudioData = reader.Reader.ReadBoolean();
                m_LoadInBackground = reader.Reader.ReadBoolean();
                m_Legacy3D = reader.Reader.ReadBoolean();
                reader.Reader.AlignStream();

                //StreamedResource m_Resource
                m_Source = reader.ReadAlignedString();
                m_Offset = reader.Reader.ReadInt64();
                m_Size = reader.Reader.ReadInt64();
                m_CompressionFormat = (AudioCompressionFormat)reader.Reader.ReadInt32();
            }

            //ResourceReader resourceReader;
            //if (!string.IsNullOrEmpty(m_Source))
            //{
            //    resourceReader = new ResourceReader(m_Source, assetsFile, m_Offset, m_Size);
            //}
            //else
            //{
            //    resourceReader = new ResourceReader(reader, reader.BaseStream.Position, m_Size);
            //}
            m_AudioData = new PartialStream(reader.Reader.BaseStream, m_Size);
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".wav", mode, out fileName))
            {
                return;
            }
            using var fs = File.Create(fileName);
            m_AudioData.CopyTo(fs, Audio.Decode);
        }
    }

    public enum FMODSoundType
    {
        UNKNOWN = 0,
        ACC = 1,
        AIFF = 2,
        ASF = 3,
        AT3 = 4,
        CDDA = 5,
        DLS = 6,
        FLAC = 7,
        FSB = 8,
        GCADPCM = 9,
        IT = 10,
        MIDI = 11,
        MOD = 12,
        MPEG = 13,
        OGGVORBIS = 14,
        PLAYLIST = 15,
        RAW = 16,
        S3M = 17,
        SF2 = 18,
        USER = 19,
        WAV = 20,
        XM = 21,
        XMA = 22,
        VAG = 23,
        AUDIOQUEUE = 24,
        XWMA = 25,
        BCWAV = 26,
        AT9 = 27,
        VORBIS = 28,
        MEDIA_FOUNDATION = 29
    }

    public enum AudioCompressionFormat
    {
        PCM = 0,
        Vorbis = 1,
        ADPCM = 2,
        MP3 = 3,
        PSMVAG = 4,
        HEVAG = 5,
        XMA = 6,
        AAC = 7,
        GCADPCM = 8,
        ATRAC9 = 9
    }
}
