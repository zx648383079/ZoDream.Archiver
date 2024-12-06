using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.LuaDecompiler
{
    public class LuaScheme: ILanguageScheme<LuaBytecode>
    {

        public LuaBytecode? Open(Stream stream, string filePath, string fileName)
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

        public void Create(Stream stream, LuaBytecode data)
        {
            using var wr = new StreamWriter(stream);
            var writer = new LuaWriter();
            writer.Write(wr, data);
        }
    }
}
