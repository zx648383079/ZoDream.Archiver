using System;
using System.IO;
using System.Text;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Language;
using ZoDream.Shared.Language.AST;
using ZoDream.Shared.Models;

namespace ZoDream.LuaDecompiler
{
    public partial class LuacReader
    {
        internal static string[] IdentItems = [
            "and", "break", "do", "else", "elseif", "end", "false", "for", "function", "if",
            "in", "local", "nil", "not", "or", "repeat", "return", "then", "true", "until",
            "while",
        ];

  
    }
}
