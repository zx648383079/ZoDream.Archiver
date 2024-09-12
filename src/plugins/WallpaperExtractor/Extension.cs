using SkiaSharp;
using System.Text;
using ZoDream.WallpaperExtractor.Models;
using ZoDream.Shared.Drawing;
using System.IO;
using System;
using System.Collections.Generic;

namespace ZoDream.WallpaperExtractor
{
    internal static class PackageExtension
    {

        public static string ReadNString(this BinaryReader reader)
        {
            var length = reader.ReadUInt32();
            if (length <= 0)
            {
                return string.Empty;
            }
            return Encoding.UTF8.GetString(reader.ReadBytes((int)length));
        }

        public static string ReadNZeroString(this BinaryReader reader, int maxLength = 16)
        {
            var buffer = new List<byte>();
            while (buffer.Count < maxLength)
            {
                var b = reader.ReadByte();
                if (b <= 0)
                {
                    break;
                }
                buffer.Add(b);
            }
            return Encoding.UTF8.GetString(buffer.ToArray());
        }

        public static void CreateDirectory(string fileName)
        {
            var folder = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        public static BitmapFormat Parse(this TexFormat format)
        {
            return format switch
            {
                TexFormat.RGBA8888 => BitmapFormat.RGBA8888,
                TexFormat.DXT5 => BitmapFormat.DXT5,
                TexFormat.DXT3 => BitmapFormat.DXT3,
                TexFormat.DXT1 => BitmapFormat.DXT1,
                TexFormat.RG88 => BitmapFormat.RG88,
                TexFormat.R8 => BitmapFormat.L8,
                _ => throw new NotSupportedException()
            };
        }

        public static string GetFileExtension(this FreeImageFormat format)
        {
            return format switch
            {
                FreeImageFormat.FIF_BMP => "bmp",
                FreeImageFormat.FIF_ICO => "ico",
                FreeImageFormat.FIF_JPEG => "jpg",
                FreeImageFormat.FIF_JNG => "jng",
                FreeImageFormat.FIF_KOALA => "koa",
                FreeImageFormat.FIF_LBM => "lbm",
                //FreeImageFormat.FIF_IFF => "iff",
                FreeImageFormat.FIF_MNG => "mng",
                FreeImageFormat.FIF_PBM or FreeImageFormat.FIF_PBMRAW => "pbm",
                FreeImageFormat.FIF_PCD => "pcd",
                FreeImageFormat.FIF_PCX => "pcx",
                FreeImageFormat.FIF_PGM or FreeImageFormat.FIF_PGMRAW => "pgm",
                FreeImageFormat.FIF_PNG => "png",
                FreeImageFormat.FIF_PPM or FreeImageFormat.FIF_PPMRAW => "ppm",
                FreeImageFormat.FIF_RAS => "ras",
                FreeImageFormat.FIF_TARGA => "tga",
                FreeImageFormat.FIF_TIFF => "tif",
                FreeImageFormat.FIF_WBMP => "wbmp",
                FreeImageFormat.FIF_PSD => "psd",
                FreeImageFormat.FIF_CUT => "cut",
                FreeImageFormat.FIF_XBM => "xbm",
                FreeImageFormat.FIF_XPM => "xpm",
                FreeImageFormat.FIF_DDS => "dds",
                FreeImageFormat.FIF_GIF => "gif",
                FreeImageFormat.FIF_HDR => "hdr",
                FreeImageFormat.FIF_FAXG3 => "g3",
                FreeImageFormat.FIF_SGI => "sgi",
                FreeImageFormat.FIF_EXR => "exr",
                FreeImageFormat.FIF_J2K => "j2k",
                FreeImageFormat.FIF_JP2 => "jp2",
                FreeImageFormat.FIF_PFM => "pfm",
                FreeImageFormat.FIF_PICT => "pict",
                FreeImageFormat.FIF_RAW => "raw",
                _ => "png",
            };
        }

        public static SKEncodedImageFormat Parse(this FreeImageFormat format)
        {
            return format switch
            {
                FreeImageFormat.FIF_BMP => SKEncodedImageFormat.Bmp,
                FreeImageFormat.FIF_ICO => SKEncodedImageFormat.Ico,
                FreeImageFormat.FIF_JPEG => SKEncodedImageFormat.Jpeg,
                FreeImageFormat.FIF_PNG => SKEncodedImageFormat.Png,
                FreeImageFormat.FIF_WBMP => SKEncodedImageFormat.Wbmp,
                FreeImageFormat.FIF_GIF => SKEncodedImageFormat.Gif,
                _ => SKEncodedImageFormat.Png,
            };
        }

        public static IImageData Decode(this TexMipmap tex, TexFormat format)
        {
            return BitmapFactory.Decode(tex.Read(), tex.Width, tex.Height, format.Parse());
        }

        public static void Decode(this TexMipmap tex, TexFormat format, 
            Stream output, FreeImageFormat imageFormat)
        {
            using var bitmap = tex.Decode(format).ToImage();
            if (bitmap == null)
            {
                return;
            }
            bitmap.Encode(output, imageFormat.Parse(), 100);
        }

        public static void Decode(this TexMipmap tex, TexFormat format,
            FreeImageFormat imageFormat, string fileName)
        {
            using var bitmap = tex.Decode(format).ToImage();
            if (bitmap == null)
            {
                return;
            }
            var extension = imageFormat.GetFileExtension();
            fileName = CombineExtension(fileName, extension);
            CreateDirectory(fileName);
            using var fs = File.Create(fileName);
            bitmap.Encode(fs, imageFormat.Parse(), 100);
        }

        public static string CombineExtension(string fileName, string extension)
        {
            var i = fileName.LastIndexOf('.');
            if (i > fileName.Length - 8)
            {
                return fileName[..(i + 1)] + extension;
            }
            else
            {
                return fileName + "." + extension;
            }
        }
    }
}
