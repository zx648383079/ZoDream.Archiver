using K4os.Compression.LZ4;
using System.IO;

namespace ZoDream.WallpaperExtractor.Models
{
    public record TexMipmap(int Width, int Height, 
        int DecompressedBytesCount,
        bool IsLZ4Compressed, Stream Data)
    {
        public byte[] Read()
        {
            var buffer = new byte[Data.Length];
            Data.ReadExactly(buffer);
            if (!IsLZ4Compressed)
            {
                return buffer;
            }
            var target = new byte[DecompressedBytesCount];
            LZ4Codec.Decode(buffer, 0, buffer.Length, target, 0, target.Length);
            return target;
        }
    }
}
