namespace ZoDream.LuaDecompiler.Models
{
    public class LuaChunk
    {
        public string Name { get; internal set; } = string.Empty;

        public LuaProtoFlags? Flags { get; internal set; }
        public ulong LineDefined { get; internal set; }
        public ulong LastLineDefined { get; internal set; }
        public byte UpValueCount { get; internal set; }

        public byte ParameterCount { get; internal set; }
        public LuaVarArgInfo? VarArg { get; internal set; }
        public byte MaxStack { get; internal set; }
        public uint[] InstructionItems { get; internal set; } = [];

        public LuaConstant[] ConstantItems { get; internal set; } = [];
        public LuaConstant[] NumberConstantItems { get; internal set; } = [];

        public LuaChunk[] PrototypeItems { get; internal set; } = [];

        public LuaDebugInfo DebugInfo { get; internal set; } = new();
    }
}
