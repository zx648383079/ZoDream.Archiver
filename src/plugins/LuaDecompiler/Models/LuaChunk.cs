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

        public (uint, uint)[] SourceLineItems { get; internal set; } = [];

        public LuaLocalVar[] LocalItems { get; internal set; } = [];

        public LuaUpValue[] UpValueItems { get; internal set; } = [];
        public string[] UpValueNameItems { get; internal set; } = [];
    }
}
