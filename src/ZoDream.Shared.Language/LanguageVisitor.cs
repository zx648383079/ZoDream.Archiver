using System.IO;
using System.Linq.Expressions;
using ZoDream.Shared.Language.AST;

namespace ZoDream.Shared.Language
{
    public abstract class LanguageVisitor : ExpressionVisitor, ILanguageWriter
    {
        protected TextWriter? Writer;

        public void Write(Stream output, GlobalExpression data)
        {
            Writer = new StreamWriter(output);
            Visit(data);
            Writer.Close();
        }
    }
}
