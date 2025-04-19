using System.IO;

namespace UnityEngine
{
    public sealed class Texture2D : Texture
    {
        public int Width;
        public int Height;
        public TextureFormat TextureFormat;
        public bool MipMap;
        public int MipCount;
        public GLTextureSettings TextureSettings;
        public Stream ImageData;
        public ResourceSource StreamData;
    }

    public struct GLTextureSettings
    {
        public int FilterMode;
        public int Aniso;
        public float MipBias;
        public int WrapMode;
        public int WrapV;
        public int WrapW;
    }
}
