using System.IO;
using ZoDream.Shared.Language.AST;

namespace ZoDream.Shared.Language
{
    public interface ILanguageWriter
    {

        public void Write(Stream output, GlobalExpression data);
    }
}
