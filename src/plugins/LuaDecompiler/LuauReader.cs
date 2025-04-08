using System;
using System.IO;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Language;

namespace ZoDream.LuaDecompiler
{
    public class LuauReader(Stream input) : ILanguageReader<LuaBytecode>
    {
        public LuaBytecode Read()
        {
            throw new NotImplementedException();
        }
    }
}
