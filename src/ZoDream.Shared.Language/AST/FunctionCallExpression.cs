using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace ZoDream.Shared.Language.AST
{
    public class FunctionCallExpression: Expression
    {
        public ReadOnlyCollection<Expression> Arguments { get; }

        public Expression Function { get; }

        internal FunctionCallExpression(string function, ReadOnlyCollection<Expression> arguments)
        {
            Arguments = arguments;
            Function = Constant(function);
        }

        public FunctionCallExpression(Expression function, ReadOnlyCollection<Expression> arguments)
        {
            Arguments = arguments;
            Function = function;
        }
    }
}
