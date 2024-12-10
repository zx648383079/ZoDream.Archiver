using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace ZoDream.Shared.Language.AST
{
    public class BatchAssignExpression: Expression
    {
        public ReadOnlyCollection<Expression> Parameters { get; }
        public ReadOnlyCollection<Expression> Values { get; }

        internal BatchAssignExpression(
            ReadOnlyCollection<Expression> parameters,
            ReadOnlyCollection<Expression> values
            )
        {
            Parameters = parameters;
            Values = values;
        }
    }
}
