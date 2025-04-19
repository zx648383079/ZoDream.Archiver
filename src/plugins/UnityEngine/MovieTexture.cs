using System.IO;

namespace UnityEngine
{
    public class MovieTexture : Texture
    {
        public Stream MovieData;
        public PPtr<AudioClip> AudioClip;
    }
}
