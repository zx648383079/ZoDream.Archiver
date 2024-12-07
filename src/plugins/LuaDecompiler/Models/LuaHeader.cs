using ZoDream.Shared.Models;

namespace ZoDream.LuaDecompiler.Models
{
    public class LuaHeader
    {
        public LuaVersion Version { get; set; }
        public byte FormatVersion { get; set; }
        public EndianType Endianness { get; set; } = EndianType.BigEndian;
        public byte SizeOfInt { get; set; }
        public byte SizeOfSizeT { get; set; }
        public byte SizeOfInstruction { get; set; }
        public byte SizeOfNumber { get; set; }
        public bool IsNumberIntegral { get; set; }
        public ulong LuacInt { get; set; }
        public double LuacNumber { get; set; }

        public byte SizeOfUpValue { get; set; }

        public LuaHeaderFlags? Flags { get; set; }
    }
}
