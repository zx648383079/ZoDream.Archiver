using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using ZoDream.Shared.Language;

namespace ZoDream.LuaDecompiler
{
    public partial class LuaWriter : LanguageVisitor, ILanguageWriter
    {

        [return: NotNullIfNotNull(nameof(node))]
        public override Expression? Visit(Expression? node)
        {
            return base.Visit(node);
        }
    }
}
