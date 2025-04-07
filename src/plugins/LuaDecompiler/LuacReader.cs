using System.IO;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Language;

namespace ZoDream.LuaDecompiler
{
    public partial class LuacReader(Stream input): ILanguageReader<LuaBytecode>
    {
        public LuaBytecode Read()
        {
            return ReadBytecode(input);
        }

    }
}
