using System.IO;
using ZoDream.Shared.Language.AST;

namespace ZoDream.Shared.Language
{
    public interface ILanguageReader
    {
        public GlobalExpression Read(Stream input);
    }
}
