using SkiaSharp;

namespace ZoDream.Shared.Drawing
{
    public class ByteImageData(byte[] buffer, int width, int height, SKColorType format) : BaseImageData
    {
        public override SKBitmap? TryParse()
        {
            var data = SKData.CreateCopy(buffer);
            var newInfo = new SKImageInfo(width, height, format);
            var bitmap = new SKBitmap();
            bitmap.InstallPixels(newInfo, data.Data);
            return bitmap;
        }
    }
}
