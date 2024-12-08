using System;
using System.ComponentModel.DataAnnotations;

namespace ZoDream.LuaDecompiler.Models
{
    public partial class OperandExtractor: IOperandExtractor
    {

        private readonly Operand[] _maps;

        public OperandFieldExtractor Op { get; private set; }
        public OperandFieldExtractor A { get; private set; }
        public OperandFieldExtractor B { get; private set; }
        public OperandFieldExtractor C { get; private set; }
        public OperandFieldExtractor k { get; private set; }
        public OperandFieldExtractor Ax { get; private set; }
        public OperandFieldExtractor sJ { get; private set; }
        public OperandFieldExtractor Bx { get; private set; }
        public OperandFieldExtractor sBx { get; private set; }
        public OperandFieldExtractor x { get; private set; }


        public Operand GetOperand(uint code)
        {
            var op = Op.Extract((int)code);
            return _maps[op];
        }

        public IOperandCode Extract([Length(4, 4)] byte[] buffer)
        {
            return Extract(BitConverter.ToUInt32(buffer));
        }

        public IOperandCode Extract(uint code)
        {
            return new OperandCode(GetOperand(code), (int)code, this);
        }

        public IOperandCode[] Extract(uint[] items)
        {
            var extraByte = new bool[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                extraByte[i] = GetOperand(items[i]) == Operand.SETLIST && C.Extract((int)items[i]) == 0;
            }
            var data = new IOperandCode[items.Length];
            for (var i = 0; i < items.Length; i++)
            {
                var op = i > 0 && extraByte[i - 1] ? Operand.EXTRABYTE : GetOperand(items[i]);
                data[i] = new OperandCode(op, (int)items[i], this);
            }
            return data;
        }

        public OperandExtractor(LuaVersion version)
        {
            _maps = version switch
            {
                LuaVersion.Lua50 => _opcodeMapV50,
                LuaVersion.Lua51 => _opcodeMapV51,
                LuaVersion.Lua52 => _opcodeMapV52,
                LuaVersion.Lua53 => _opcodeMapV53,
                LuaVersion.Lua54 or LuaVersion.Lua54Beta => _opcodeMapV54,
                _ => throw new NotImplementedException()
            };
            switch (version)
            {
                case LuaVersion.Lua51 or LuaVersion.Lua52:
                    Op = new(6, 0);
                    A = new(8, 6);
                    B = new(9, 23);
                    C = new(9, 14);
                    k = new();
                    Ax = new(26, 6);
                    sJ = new();
                    Bx = new(18, 14);
                    sBx = new(18, 14, 131071);
                    x = new(32, 0);
                    break;
                case LuaVersion.Lua53 or LuaVersion.Lua54 or LuaVersion.Lua54Beta:
                    Op = new(7, 0);
                    A = new(8, 7);
                    B = new(8, 16);
                    C = new(8, 24);
                    k = new(1, 15);
                    Ax = new(25, 7);
                    sJ = new(25, 7, (1 << 24) - 1);
                    Bx = new(17, 15);
                    sBx = new(17, 15, (1 << 16) - 1);
                    x = new(32, 0);
                    break;
                default:
                    throw new ArgumentException();
            }
        }
    }

    public class OperandFieldExtractor(int size, int shift, int offset)
    {
        public OperandFieldExtractor(int size, int shift)
            : this(size, shift, 0)
        { }

        public OperandFieldExtractor()
            : this(0, 0, 0)
        {
            
        }


        private readonly int _mask = (int)((1L << size) - 1);

        public int Max => _mask - offset;

        public int Extract(int codepoint)
        {
            if (size == 0)
            {
                return 0;
            }
            return ((codepoint >>> shift) & _mask) - offset;
        }
    }
}
