using SkiaSharp;

namespace ZoDream.Shared.Drawing
{
    public class FileImageData(string fileName) : BaseImageData
    {
        public override SKBitmap? TryParse()
        {
            return SKBitmap.Decode(fileName);
        }
    }
}
