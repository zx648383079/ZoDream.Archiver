using System.IO;

namespace UnityEngine
{
    public sealed class AudioClip : Object
    {
        public int Format;
        public AudioType Type;
        public bool Is3D;
        public bool UseHardware;

        //version 5
        public int LoadType;
        public int Channels;
        public int Frequency;
        public int BitsPerSample;
        public float Length;
        public bool IsTrackerFormat;
        public int SubSoundIndex;
        public bool PreloadAudioData;
        public bool LoadInBackground;
        public bool Legacy3D;
        public AudioCompressionFormat CompressionFormat;

        public string Source;
        public long Offset; //ulong
        public long Size; //ulong
        public Stream AudioData;
    }
}
