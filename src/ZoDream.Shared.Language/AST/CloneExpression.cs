using System.Linq.Expressions;

namespace ZoDream.Shared.Language.AST
{
    public class CloneExpression: Expression
    {
        public Expression Value { get; }
        internal CloneExpression(Expression value)
        {
            Value = value;
        }
    }
}
