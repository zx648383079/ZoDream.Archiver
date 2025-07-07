namespace Il2CppDumper
{
    public interface IDumpOptions
    {
        public bool DumpMethod { get; }
        public bool DumpField { get; }
        public bool DumpProperty { get; }
        public bool DumpAttribute { get; }
        public bool DumpFieldOffset { get; }
        public bool DumpMethodOffset { get; }
        public bool DumpTypeDefIndex { get; }
        public bool GenerateDummyDll { get; }
        public bool GenerateStruct { get; }
        public bool DummyDllAddToken { get; }
        public bool RequireAnyKey { get; }
        public bool ForceIl2CppVersion { get; }
        public double ForceVersion { get; }
        public bool ForceDump { get; }
        public bool NoRedirectedPointer { get; }
    }
}
