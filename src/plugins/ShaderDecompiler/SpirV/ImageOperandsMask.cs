namespace ZoDream.ShaderDecompiler.SpirV
{
    public enum ImageOperandsMask
    {
        MaskNone = 0,
        Bias = 0x00000001,
        Lod = 0x00000002,
        Grad = 0x00000004,
        ConstOffset = 0x00000008,
        Offset = 0x00000010,
        ConstOffsets = 0x00000020,
        Sample = 0x00000040,
        MinLod = 0x00000080,
    }
}
