using System.IO;

namespace UnityEngine
{
    public sealed class Texture3D : Texture
    {
        public int Width;
        public int Height;
        public TextureFormat TextureFormat;
        public bool MipMap;
        public int MipCount;
        public GLTextureSettings TextureSettings;
        public Stream ImageData;

        public int Depth { get; set; }
    }
}
