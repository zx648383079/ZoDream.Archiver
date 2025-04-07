using System;
using System.IO;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Language;

namespace ZoDream.LuaDecompiler
{
    public class LuaScheme: ILanguageScheme<LuaBytecode>
    {

        public LuaBytecode? Open(Stream stream)
        {
            var pos = stream.Position;
            var buffer = new byte[4];
            stream.ReadExactly(buffer, 0, 4);
            stream.Seek(pos, SeekOrigin.Begin);
            if (buffer.StartsWith(LuacReader.Signature))
            {
                return new LuacReader(stream).Read();
            }
            if (buffer.StartsWith(LuaJitReader.Signature))
            {
                return new LuaJitReader(stream).Read();
            }
            return null;
        }

        public void Create(Stream stream, LuaBytecode data)
        {
            new LuaWriter(data).Write(stream);
        }

    }
}
