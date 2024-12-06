namespace ZoDream.LuaDecompiler.Models
{
    public class LuaHeaderFlags
    {
        public bool IsBigEndian { get; set; }
        public bool IsStripped { get; set; }
        public bool HasFFI { get; set; }
        public bool FFr2 { get; set; }

        public LuaHeaderFlags(byte code)
        {
            IsBigEndian = (code & 0b1) != 0;
            IsStripped = (code & 0b10) != 0;
            HasFFI = (code & 0b1000) != 0;
            FFr2  = (code & 0b10000000) != 0;
        }
    }
}
