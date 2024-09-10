using System.IO;
using ZoDream.Shared.RustWrapper;

namespace ZoDream.WallpaperExtractor.Models
{
    public record TexMipmap(int Width, int Height, 
        int DecompressedBytesCount,
        bool IsLZ4Compressed, Stream Data)
    {

        public byte[] Read()
        {
            var buffer = new byte[Data.Length];
            Data.Read(buffer, 0, buffer.Length);
            if (!IsLZ4Compressed)
            {
                return buffer;
            }
            return CompressHelper.Lz4Decompress(buffer, DecompressedBytesCount);
        }
    }
}
