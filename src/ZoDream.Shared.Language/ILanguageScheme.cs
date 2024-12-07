using System.IO;
using ZoDream.Shared.Language.AST;

namespace ZoDream.Shared.Language
{
    public interface ILanguageScheme
    {
        public void Create(Stream stream, GlobalExpression data);
        public GlobalExpression? Open(Stream stream, string filePath, string fileName);
    }
}
