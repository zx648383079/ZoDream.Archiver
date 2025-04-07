namespace ZoDream.ShaderDecompiler.SpirV
{
    public enum MemorySemanticsShift
    {
        Acquire = 1,
        Release = 2,
        AcquireRelease = 3,
        SequentiallyConsistent = 4,
        UniformMemory = 6,
        SubgroupMemory = 7,
        WorkgroupMemory = 8,
        CrossWorkgroupMemory = 9,
        AtomicCounterMemory = 10,
        ImageMemory = 11,
    }
}
