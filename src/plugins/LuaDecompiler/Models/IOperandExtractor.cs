using System.ComponentModel.DataAnnotations;

namespace ZoDream.LuaDecompiler.Models
{
    public interface IOperandExtractor
    {
        public IOperandCode Extract([Length(4, 4)] byte[] buffer);

        public IOperandCode Extract(uint code);
    }
}
