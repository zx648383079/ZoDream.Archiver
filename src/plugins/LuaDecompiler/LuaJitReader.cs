using System;
using System.Collections.Generic;
using System.IO;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Language;
using ZoDream.Shared.Language.AST;

namespace ZoDream.LuaDecompiler
{
    public partial class LuaJitReader : ILanguageReader
    {
        public GlobalExpression Read(Stream input)
        {
            var data = ReadBytecode(input);
            return CreateBuilder(data);
        }

        private GlobalExpression CreateBuilder(LuaBytecode data)
        {
            var builder = new GlobalExpression();

            return builder;
        }
    }
}
