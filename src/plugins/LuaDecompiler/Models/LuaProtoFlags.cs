namespace ZoDream.LuaDecompiler.Models
{
    public class LuaProtoFlags
    {
        public bool HasChild { get; set; }
        public bool IsVariadic { get; set; }
        public bool HasFFI { get; set; }
        public bool JitDisabled { get; set; }
        public bool HasILoop { get; set; }

        public LuaProtoFlags(byte code)
        {
            HasChild = (code & 0b1) != 0;
            IsVariadic = (code & 0b10) != 0;
            HasFFI = (code & 0b100) != 0;
            JitDisabled = (code & 0b1000) != 0;
            HasILoop = (code & 0b10000) != 0;
        }
    }
}
