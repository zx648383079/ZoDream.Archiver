using System.IO;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Language;

namespace ZoDream.LuaDecompiler
{
    public partial class LuaJitReader(Stream input) : ILanguageReader<LuaBytecode>
    {
        public LuaBytecode Read()
        {
            return ReadBytecode(input);
        }
    }
}
