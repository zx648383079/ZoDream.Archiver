using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using ZoDream.Shared.Drawing.Gif;

namespace ZoDream.Shared.Drawing
{
    internal class GifFrame(bool firstFrame, int palSize = 7)
    {
        public List<byte> ColorTable { get; private set; } = [];
        public IList<byte> ImageData { get; private set; } = [];
        public List<byte> ImageDescriptor { get; private set; } = new List<byte>(10);
        
        public void Load(SKImage bitmap, int quality = 100)
        {
            Load(bitmap, SKRectI.Create(0, 0, bitmap.Width, bitmap.Height), quality);
        }
        public void Load(SKImage bitmap, SKRectI rect, int quality = 100)
        {
            if (rect.Width != bitmap.Width || rect.Height != bitmap.Height)
            {
                bitmap = bitmap.Resize(new SKImageInfo(rect.Width, rect.Height), SKFilterQuality.High
                //    quality switch
                //{
                //    >= 80 => SKFilterQuality.High,
                //    >= 40 => SKFilterQuality.Medium,
                //    _ => SKFilterQuality.Low,
                //}
                )!;
                if (bitmap is null)
                {
                    return;
                }
            }
            var pixels = new byte[3 * bitmap.Width * bitmap.Height];
            int count = 0;
            var pixmap = bitmap.PeekPixels();
            for (int th = 0; th < bitmap.Height; th++)
            {
                for (int tw = 0; tw < bitmap.Width; tw++)
                {
                    var color = pixmap.GetPixelColor(tw, th);
                    pixels[count] = color.Red;
                    count++;
                    pixels[count] = color.Green;
                    count++;
                    pixels[count] = color.Blue;
                    count++;
                }
            }
            int len = pixels.Length;
            int nPix = len / 3;
            var indexedPixels = new byte[nPix];
            var nq = new NeuQuant(pixels, len, quality);
            // initialize quantizer
            ColorTable.Clear();
            ColorTable.AddRange(nq.Process()); // create reduced palette
                                               // convert map from BGR to RGB
                                               //			for (int i = 0; i < colorTab.Length; i += 3) 
                                               //			{
                                               //				byte temp = colorTab[i];
                                               //				colorTab[i] = colorTab[i + 2];
                                               //				colorTab[i + 2] = temp;
                                               //				usedEntry[i / 3] = false;
                                               //			}
                                               // map image pixels to new palette
            int n = (3 * 256) - ColorTable.Count;
            for (int i = 0; i < n; i++)
            {
                ColorTable.Add(0);
            }

            int k = 0;
            for (int i = 0; i < nPix; i++)
            {
                int index =
                    nq.Map(pixels[k++] & 0xff,
                    pixels[k++] & 0xff,
                    pixels[k++] & 0xff);
                // usedEntry[index] = true;
                indexedPixels[i] = (byte)index;
            }

            var colorDepth = 8;
            //var palSize = 7;
            using var ms = new MemoryStream();
            var encoder =
                new LZWEncoder(bitmap.Width, bitmap.Height, indexedPixels, colorDepth);
            encoder.Encode(ms);
            ImageData = ms.ToArray();
            // get closest match to transparent color if specified
            //if (transparent != Color.Empty)
            //{
            //    //transIndex = FindClosest(transparent);
            //    transIndex = nq.Map(transparent.B, transparent.G, transparent.R);
            //}

            #region Desc
            ImageDescriptor.Clear();
            ImageDescriptor.Add(0x2c);
            ImageDescriptor.AddRange(ConvertShort(rect.Left)); // x
            ImageDescriptor.AddRange(ConvertShort(rect.Right)); // y
            ImageDescriptor.AddRange(ConvertShort(bitmap.Width)); // width
            ImageDescriptor.AddRange(ConvertShort(bitmap.Height)); // height
            if (firstFrame)
            {
                // no LCT  - GCT is used for first (or only) frame
                ImageDescriptor.Add(0);
            }
            else
            {
                // specify normal LCT
                ImageDescriptor.Add(Convert.ToByte(0x80 | // 1 local color table  1=yes
                    0 | // 2 interlace - 0=no
                    0 | // 3 sorted - 0=no
                    0 | // 4-5 reserved
                    palSize)); // 6-8 size of color table
            }
            #endregion


        }

        internal static byte[] ConvertShort(int val)
        {
            return [(byte)(val & 0xff), (byte)((val >> 8) & 0xff)];
        }
    }
}
