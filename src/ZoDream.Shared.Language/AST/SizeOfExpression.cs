using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace ZoDream.Shared.Language.AST
{
    public class SizeOfExpression: Expression
    {
        public Expression Expression { get; }

        internal SizeOfExpression(Expression expression)
        {
            Expression = expression;
        }
    }
}
