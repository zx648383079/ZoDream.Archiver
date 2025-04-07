namespace ZoDream.ShaderDecompiler.SpirV
{
    public class SpvHeader
    {
        public uint Version { get; set; }
        public uint Generator { get; set; }
        public uint Bound { get; set; }
        public uint Reserved { get; set; }

        public int MajorVersion => (int)(Version >> 16);
        public int MinorVersion => (int)((Version >> 8) & 0xFF);

        public int GeneratorId => (int)(Generator >> 16);
        public int GeneratorVersion => (int)(Generator & 0xFFFF);
    }
}
