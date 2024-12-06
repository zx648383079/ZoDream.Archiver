using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.LuaDecompiler
{
    public partial class LuaWriter : ILanguageWriter
    {

        private static string[] IdentItems = [
            "and", "break", "do", "else", "elseif", "end", "false", "for", "function", "if",
            "in", "local", "nil", "not", "or", "repeat", "return", "then", "true", "until",
            "while",
        ];

        public void Write(TextWriter writer, LuaBytecode data)
        {

        }
    }
}
