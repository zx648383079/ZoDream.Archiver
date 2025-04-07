using System.IO;
using ZoDream.ShaderDecompiler.SpirV;
using ZoDream.Shared.Language;

namespace ZoDream.ShaderDecompiler
{
    public class ShaderScheme : ILanguageScheme<SpvBytecode>
    {
       

        public void Create(Stream stream, SpvBytecode data)
        {
            new SpirVWriter(data).Write(stream);
        }

        public SpvBytecode? Open(Stream stream)
        {
            var pos = stream.Position;
            var buffer = new byte[4];
            stream.ReadExactly(buffer, 0, 4);
            stream.Seek(pos, SeekOrigin.Begin);
            if (SmolvReader.IsSupport(buffer, 4))
            {
                return new SmolvReader(stream).Read();
            }
            if (SpirVReader.IsSupport(buffer, 4))
            {
                return new SpirVReader(stream).Read();
            }
            return null;
        }

        public static void Disassemble(Stream input, ICodeWriter output)
        {
            var scheme = new ShaderScheme();
            var data = scheme.Open(input);
            if (data is not null)
            {
                new SpirVWriter(data).Decompile(output);
            }
        }
    }
}
