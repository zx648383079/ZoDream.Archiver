using System.IO;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Compression.Own
{
    public class OwnFileHeader: IArchiveHeader
    {
        public OwnFileHeader()
        {
            
        }
        public OwnFileHeader(bool multiple)
        {
            WithName = true;
            Multiple = multiple;
        }

        public bool WithName { get; set; }
        public bool Multiple { get; set; }
        /// <summary>
        /// 这个版本是指包含一些运算公式
        /// </summary>
        public OwnVersion Version { get; set; } = OwnVersion.Unknown;

        public byte VersionByte 
        {
            get {
                var b = (byte)Version;
                return b >= 0x0A ? (byte)(b + 1) : b;
            }
            set => Version = (OwnVersion)(value > 0x0A ? (byte)(value - 1) : value);
        }

        public void Read(Stream input)
        {
            var buffer = new byte[4];
            input.Read(buffer, 0, 4);
            if (buffer[0] != 0x23 || buffer[1] != 0x5A)
            {
                throw new NotSupportedException("Own Archive Header error");
            }
            WithName = buffer[2] != 0x2;
            Multiple = buffer[2] == 0x3;
            if (buffer[3] == 0x0A)
            {
                return;
            }
            VersionByte = buffer[3];
            if (input.ReadByte() != 0x0A)
            {
                throw new NotSupportedException("Own Archive Header error");
            }
        }

        public void Write(Stream output) 
        {
            output.WriteByte(0x23);
            output.WriteByte(0x5A);
            if (Multiple)
            {
                output.WriteByte(3);
            }
            else
            {
                output.WriteByte((byte)(WithName ? 1 : 2));
            }
            if (Version > 0)
            {
                output.WriteByte(VersionByte);
            }
            output.WriteByte(0x0A);
        }
    }
}
