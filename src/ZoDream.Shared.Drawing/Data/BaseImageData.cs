using SkiaSharp;

namespace ZoDream.Shared.Drawing
{
    public abstract class BaseImageData: IImageData
    {

        public abstract SKBitmap? TryParse();
    }
}
