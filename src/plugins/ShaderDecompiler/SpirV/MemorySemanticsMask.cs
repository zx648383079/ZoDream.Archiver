namespace ZoDream.ShaderDecompiler.SpirV
{
    public enum MemorySemanticsMask
    {
        MaskNone = 0,
        Acquire = 0x00000002,
        Release = 0x00000004,
        AcquireRelease = 0x00000008,
        SequentiallyConsistent = 0x00000010,
        UniformMemory = 0x00000040,
        SubgroupMemory = 0x00000080,
        WorkgroupMemory = 0x00000100,
        CrossWorkgroupMemory = 0x00000200,
        AtomicCounterMemory = 0x00000400,
        ImageMemory = 0x00000800,
    }
}
