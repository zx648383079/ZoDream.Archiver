namespace ZoDream.ShaderDecompiler.SpirV
{
    public enum MemoryAccessMask
    {
        MaskNone = 0,
        Volatile = 0x00000001,
        Aligned = 0x00000002,
        Nontemporal = 0x00000004,
    }
}
