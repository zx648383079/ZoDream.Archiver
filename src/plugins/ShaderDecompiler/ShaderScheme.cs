using System;
using System.IO;
using ZoDream.Shared.IO;
using ZoDream.Shared.Language;
using ZoDream.Shared.Language.AST;

namespace ZoDream.ShaderDecompiler
{
    public class ShaderScheme : ILanguageScheme
    {
       

        public void Create(Stream stream, GlobalExpression data)
        {
            throw new NotImplementedException();
        }

        public GlobalExpression? Open(Stream stream, string filePath, string fileName)
        {
            var pos = stream.Position;
            var buffer = new byte[4];
            stream.ReadExactly(buffer, 0, 4);
            stream.Seek(pos, SeekOrigin.Begin);
            if (SmolvReader.IsSupport(buffer, 4))
            {
                return new SmolvReader().Read(stream);
            }
            if (SpirVReader.IsSupport(buffer, 4))
            {
                return new SpirVReader().Read(stream);
            }
            return null;
        }

        public static string Disassemble(PartialStream stream)
        {
            var pos = stream.Position;
            var buffer = new byte[4];
            stream.ReadExactly(buffer, 0, 4);
            stream.Seek(pos, SeekOrigin.Begin);
            if (SmolvReader.IsSupport(buffer, 4))
            {
                return new SpirVWriter(new SmolvReader().ReadBytecode(stream)).ToString();
            }
            if (SpirVReader.IsSupport(buffer, 4))
            {
                return new SpirVWriter(new SpirVReader().ReadBytecode(stream)).ToString();
            }
            return string.Empty;
        }
    }
}
