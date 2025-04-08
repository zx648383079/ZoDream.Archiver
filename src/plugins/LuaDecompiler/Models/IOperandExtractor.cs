using System.ComponentModel.DataAnnotations;
using ZoDream.Shared.Language;

namespace ZoDream.LuaDecompiler.Models
{
    public interface IOperandExtractor
    {
        public ILanguageOpcode Extract([Length(4, 4)] byte[] buffer);

        public ILanguageOpcode Extract(uint code);
    }
}
