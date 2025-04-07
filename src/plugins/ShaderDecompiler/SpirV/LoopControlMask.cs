namespace ZoDream.ShaderDecompiler.SpirV
{
    public enum LoopControlMask
    {
        MaskNone = 0,
        Unroll = 0x00000001,
        DontUnroll = 0x00000002,
        DependencyInfinite = 0x00000004,
        DependencyLength = 0x00000008,
    }
}
