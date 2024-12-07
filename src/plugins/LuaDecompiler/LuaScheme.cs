using System;
using System.IO;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Language;
using ZoDream.Shared.Language.AST;

namespace ZoDream.LuaDecompiler
{
    public class LuaScheme: ILanguageScheme
    {

        public GlobalExpression? Open(Stream stream, string filePath, string fileName)
        {
            var pos = stream.Position;
            var buffer = new byte[4];
            stream.ReadExactly(buffer, 0, 4);
            stream.Seek(pos, SeekOrigin.Begin);
            if (buffer.StartsWith(LuacReader.Signature))
            {
                return new LuacReader().Read(stream);
            }
            if (buffer.StartsWith(LuaJitReader.Signature))
            {
                return new LuaJitReader().Read(stream);
            }
            return null;
        }

        public void Create(Stream stream, GlobalExpression data)
        {
            new LuaWriter().Write(stream, data);
        }

    }
}
