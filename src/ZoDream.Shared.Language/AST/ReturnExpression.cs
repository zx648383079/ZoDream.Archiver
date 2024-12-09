using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace ZoDream.Shared.Language.AST
{
    public class ReturnExpression: Expression
    {
        public ReadOnlyCollection<Expression> Values { get; }

        internal ReturnExpression(ReadOnlyCollection<Expression> values)
        {
            Values = values;
        }
    }
}
