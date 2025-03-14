using SkiaSharp;
using System;
using System.Numerics;

namespace ZoDream.Shared.Drawing
{
    /// <summary>
    /// 凹凸贴图转法向贴图
    /// https://github.com/rob5300/ssbumpToNormal-Win
    /// </summary>
    public static class BumpToNormal
    {
        private static readonly float _OO_SQRT_3 = 0.57735025882720947f;
        private static readonly Vector3[] _bumpBasisTranspose = [
            new( 0.81649661064147949f, -0.40824833512306213f, -0.40824833512306213f ),
            new(  0.0f, 0.70710676908493042f, -0.7071068286895752f ),
            new(  _OO_SQRT_3, _OO_SQRT_3, _OO_SQRT_3 )
        ];
        
        public static SKImage Convert(SKImage source)
        {
            var buffer = new byte[source.Width * source.Height * 3];
            using var srcPixel = source.PeekPixels();
            for (var y = 0; y < srcPixel.Height; y++)
            {
                for (var x = 0; x < srcPixel.Width; x++)
                {
                    var pixel = srcPixel.GetPixelColor(x, y);
                    var readVector = new Vector3(pixel.Red / 255f, pixel.Green / 255f, pixel.Blue / 255f);
                    var offset = (y * srcPixel.Width + x) * 3;
                    for (var i = 0; i < 3; i++)
                    {
                        buffer[offset + i] = ConvertVector(ref readVector, i);
                    }
                }
            }
            var newInfo = new SKImageInfo(srcPixel.Width, srcPixel.Height, SKColorType.Rgb888x, SKAlphaType.Opaque);
            var data = SKData.CreateCopy(buffer);
            return SKImage.FromPixels(newInfo, data);
        }

        private static byte ConvertVector(ref Vector3 vecIn, int index)
        {
            var newColor = Vector3.Dot(vecIn, _bumpBasisTranspose[index]) * 0.5f + 0.5f;
            return (byte)Math.Floor(Math.Clamp(newColor, 0f, 1f) * 255f);
        }
    }
}
