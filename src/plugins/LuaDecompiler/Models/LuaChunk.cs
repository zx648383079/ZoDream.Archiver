using ZoDream.Shared.Language;

namespace ZoDream.LuaDecompiler.Models
{
    public class LuaChunk : ILanguageChunk, IInstruction
    {
        public string Name { get; internal set; } = string.Empty;

        public LuaProtoFlags? Flags { get; internal set; }
        public ulong LineDefined { get; internal set; }
        public ulong LastLineDefined { get; internal set; }
        public byte UpValueCount { get; internal set; }

        public byte ParameterCount { get; internal set; }
        public LuaVarArgInfo? VarArg { get; internal set; }
        public byte MaxStack { get; internal set; }
        public ILanguageOpcode[] OpcodeItems { get; internal set; } = [];

        public LuaConstant[] ConstantItems { get; internal set; } = [];
        public LuaConstant[] NumberConstantItems { get; internal set; } = [];

        public LuaChunk[] PrototypeItems { get; internal set; } = [];

        public LuaDebugInfo DebugInfo { get; internal set; } = new();


        #region MoveCursor
        public int CurrentIndex { get; private set; } = -1;
        public ILanguageOpcode CurrentOpcode => OpcodeItems[CurrentIndex];
        public bool CanMoveNext => CurrentIndex < OpcodeItems.Length - 1;

        public IInstructionOperand[] OperandItems => [(IInstructionOperand)CurrentOpcode];

        public string Mnemonic => CurrentOpcode.ToString();

        public bool MoveNext()
        {
            if (!CanMoveNext)
            {
                return false;
            }
            CurrentIndex++;
            return true;
        }
        #endregion

    }
}
