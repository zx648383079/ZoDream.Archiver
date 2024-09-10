using SkiaSharp;

namespace ZoDream.Shared.Drawing
{
    public class BitmapImageData(SKBitmap bitmap): BaseImageData
    {
        public override SKBitmap? TryParse()
        {
            return bitmap.Copy();
        }
    }
}
