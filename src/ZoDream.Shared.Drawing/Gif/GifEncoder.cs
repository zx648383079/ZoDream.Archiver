using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Shared.Drawing
{
    public class GifEncoder(
        Stream writer,
        int width = 0,
        int height = 0,
        int delay = 33, 
        int repeat = 0): IDisposable
    {

        private bool _createdHeader;
        private bool _createdFooter;
        private int _width = width;
        private int _height = height;
        private readonly int _palSize = 7;
        public int Delay { get; private set; } = delay;
        public int Repeat { get; private set; } = repeat;
        public int FrameCount { get; private set; }


        public void AddFrame(SKBitmap bitmap,
            int delay = -1,
            int quality = 100)
        {
            if (bitmap is null)
            {
                return;
            }
            AddFrame(SKImage.FromBitmap(bitmap), delay, quality);
        }

        public void AddFrame(SKImage? bitmap,
            int delay = -1,
            int quality = 100)
        {
            if (bitmap is null)
            {
                return;
            }
            AddFrame(bitmap, SKRectI.Create(0, 0, bitmap.Width, bitmap.Height), delay, quality);
        }

        public void AddFrame(SKImage bitmap, 
            SKRectI rect,
            int delay = -1, 
            int quality = 100)
        {
            if (_width <= 0 || _height <= 0)
            {
                _width = rect.Width;
                _height = rect.Height;
            }
            var frame = new GifFrame(FrameCount < 1, _palSize);
            frame.Load(bitmap, rect, quality);

            if (!_createdHeader)
            {
                WriteHeader();
                WriteScreenDescriptor();
                writer.Write(frame.ColorTable.ToArray());
                WriteApplicationExtensionBlock(Repeat);
                _createdHeader = true;
            }

            WriteGraphicsControlExtensionBlock(delay > -1 ? delay : Delay);
            writer.Write(frame.ImageDescriptor.ToArray());
            if (FrameCount > 0)
            {
                writer.Write(frame.ColorTable.ToArray());
            }
            writer.Write(frame.ImageData.ToArray());

            FrameCount ++;
        }

        private void WriteScreenDescriptor()
        {
            // logical screen size
            writer.Write(GifFrame.ConvertShort(_width));
            writer.Write(GifFrame.ConvertShort(_height));
            // packed fields
            writer.WriteByte(Convert.ToByte(0x80 | // 1   : global color table flag = 1 (gct used)
                0x70 | // 2-4 : color resolution = 7
                0x00 | // 5   : gct sort flag = 0
                _palSize)); // 6-8 : gct size

            writer.WriteByte(0); // background color index
            writer.WriteByte(0); // pixel aspect ratio - assume 1:1
        }

        private void WriteHeader()
        {
            writer.Write([(byte)'G', (byte)'I', (byte)'F', (byte)'8', (byte)'9', (byte)'a']);
        }

        private void WriteApplicationExtensionBlock(int repeat)
        {
            writer.Write([
                0x21, // Extension introducer
                0xFF, // Application extension
                0x0B, // Size of block
                (byte)'N', // NETSCAPE2.0
                (byte)'E',
                (byte)'T',
                (byte)'S',
                (byte)'C',
                (byte)'A',
                (byte)'P',
                (byte)'E',
                (byte)'2',
                (byte)'.',
                (byte)'0',
                0x03, // Size of block
                0x01, // Loop indicator
                (byte)(repeat % 0x100), // Number of repetitions
                (byte)(repeat / 0x100), // 0 for endless loop
                0x00, // Block terminator
                ]);
        }

        private void WriteGraphicsControlExtensionBlock(int delay)
        {
            writer.Write([
                0x21, // Extension introducer
                0xF9, // Graphic control extension
                0x04, // Size of block
                0x09, // Flags: reserved, disposal method, user input, transparent color
                (byte)(delay / 10 % 0x100), // Delay time low byte
                (byte)(delay / 10 / 0x100), // Delay time high byte
                0xFF, // Transparent color index
                0x00, // Block terminator
            ]);
        }

        public void Close()
        {
            if (!_createdFooter)
            {
                writer.WriteByte(0x3B);
                _createdFooter = true;
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
