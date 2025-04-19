using System.Collections.Generic;
using System.IO;

namespace UnityEngine
{
    public sealed class Texture2DArray : Texture
    {
        public int Width;
        public int Height;
        public int Depth;
        public GraphicsFormat Format;
        public int MipCount;
        public uint DataSize;
        public GLTextureSettings TextureSettings;
        public int ColorSpace;
        public Stream ImageData;
        public ResourceSource StreamData;
        public List<Texture2D> TextureList;
    }
}
