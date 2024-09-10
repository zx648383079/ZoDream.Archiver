using SkiaSharp;

namespace ZoDream.Shared.Drawing
{
    public class FileByteImageData(byte[] buffer) : BaseImageData
    {
        public override SKBitmap? TryParse()
        {
            return SKBitmap.Decode(buffer);
        }
    }
}
