using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Live2dExporter.Models
{
    internal class MocCanvasFlags(byte code)
    {
        public bool ReverseYCoordinate { get; private set; } = (code & 0x1) != 0;
        public byte Reserved { get; private set; } = (byte)((code >> 1) & 0x1111111);
        
    }

    internal class MocCanvasInfo
    {
        public float PixelsPerUnit { get; private set; }
        public float OriginX { get; private set; }
        public float OriginY { get; private set; }
        public float CanvasWidth { get; private set; }
        public float CanvasHeight { get; private set; }
        public MocCanvasFlags CanvasFlags { get; private set; }

        public void Read(BinaryReader reader)
        {
            var ptr = reader.ReadUInt32();
            var pos = reader.BaseStream.Position;
            reader.BaseStream.Seek(ptr, SeekOrigin.Begin);
            PixelsPerUnit = reader.ReadSingle();
            OriginX = reader.ReadSingle();
            OriginY = reader.ReadSingle();
            CanvasWidth = reader.ReadSingle();
            CanvasHeight = reader.ReadSingle();
            CanvasFlags = new MocCanvasFlags(reader.ReadByte());
            reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        }
    }
}
