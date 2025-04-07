namespace ZoDream.ShaderDecompiler.SpirV
{
    public enum FPFastMathModeMask
    {
        MaskNone = 0,
        NotNaN = 0x00000001,
        NotInf = 0x00000002,
        NSZ = 0x00000004,
        AllowRecip = 0x00000008,
        Fast = 0x00000010,
    }
}
