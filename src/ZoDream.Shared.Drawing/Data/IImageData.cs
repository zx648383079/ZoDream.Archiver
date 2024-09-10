using SkiaSharp;

namespace ZoDream.Shared.Drawing
{
    public interface IImageData
    {
        public SKBitmap? TryParse();
    }
}
