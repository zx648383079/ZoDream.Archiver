using SkiaSharp;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ZoDream.Shared.Drawing
{
    public class ByteImageData(byte[] buffer, int width, int height, SKColorType format) : BaseImageData
    {
        public override SKBitmap? TryParse()
        {
            var newInfo = new SKImageInfo(width, height, format);
            var data = SKData.CreateCopy(buffer);
            //return SKBitmap.Decode(data, newInfo);
            //using MemoryStream ms = new(_buffer);
            //using SKManagedStream skStream = new(ms, false);
            //var data = SKData.Create(skStream);
            //var data = SKData.CreateCopy(_buffer);
            //var gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var bitmap = new SKBitmap();
            bitmap.InstallPixels(newInfo, data.Data);
            //bitmap.InstallPixels(newInfo, gcHandle.AddrOfPinnedObject(), newInfo.RowBytes, delegate { 
            //    gcHandle.Free();
            //}, null);
            return bitmap;
        }
    }
}
