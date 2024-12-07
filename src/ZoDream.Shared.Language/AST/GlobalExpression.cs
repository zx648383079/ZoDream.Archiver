using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace ZoDream.Shared.Language.AST
{
    public class GlobalExpression: Expression
    {
        public List<Expression> Expressions { get; private set; } = [];

        public List<ParameterExpression> Variables { get; private set; } = [];

        public List<FunctionExpression> Functions { get; private set; } = [];


        public void Add(Expression expression)
        {
            if (expression is ParameterExpression p)
            {
                Variables.Add(p);
                return;
            }
            if (expression is FunctionExpression f)
            {
                Functions.Add(f);
                return;
            }
            Expressions.Add(expression);
        }

        public void AddFunction(string name, Expression body, IEnumerable<ParameterExpression>? parameters)
        {
            if (parameters is null)
            {
                AddFunction(name, body);
                return;
            }
            Functions.Add(new FunctionExpression(name, new ReadOnlyCollection<ParameterExpression>(parameters.ToArray()), body, typeof(void)));
        }

        public void AddFunction(string name, Expression body)
        {
            Functions.Add(new FunctionExpression(name, ReadOnlyCollection<ParameterExpression>.Empty, body, typeof(void)));
        }

        public static FunctionCallExpression FunctionCall(string name, IEnumerable<Expression>? arguments)
        {
            if (arguments is null)
            {
                return FunctionCall(name);
            }
            return new FunctionCallExpression(name, new ReadOnlyCollection<Expression>(arguments.ToArray()));
        }

        public static FunctionCallExpression FunctionCall(string name)
        {
            return new FunctionCallExpression(name, ReadOnlyCollection<Expression>.Empty);
        }
    }
}
