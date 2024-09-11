using K4os.Compression.LZ4;
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
            var target = new byte[DecompressedBytesCount];
            LZ4Codec.Decode(buffer, target);
            return target;
            //var target = CompressHelper.Lz4Decompress(buffer, DecompressedBytesCount);
            //return target;
        }
    }
}
