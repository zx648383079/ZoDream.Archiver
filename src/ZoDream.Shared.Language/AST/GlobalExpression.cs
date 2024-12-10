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
            Functions.Add(Function(name, body, parameters));
        }

        public void AddFunction(string name, Expression body)
        {
            Functions.Add(Function(name, body));
        }

        public static FunctionExpression Function(string name, Expression body, IEnumerable<ParameterExpression>? parameters)
        {
            if (parameters is null)
            {
                return Function(name, body);
            }
            return new FunctionExpression(name, new ReadOnlyCollection<ParameterExpression>(parameters.ToArray()), body, typeof(void));
        }

        public static FunctionExpression Function(string name, Expression body)
        {
            return new FunctionExpression(name, ReadOnlyCollection<ParameterExpression>.Empty, body, typeof(void));
        }

        public static FunctionCallExpression FunctionCall(string name, IEnumerable<Expression>? arguments)
        {
            if (arguments is null)
            {
                return FunctionCall(name);
            }
            return new FunctionCallExpression(name, new ReadOnlyCollection<Expression>(arguments.ToArray()));
        }

        public static FunctionCallExpression FunctionCall(Expression name, IEnumerable<Expression> arguments)
        {
            return new FunctionCallExpression(name, new ReadOnlyCollection<Expression>(arguments.ToArray()));
        }

        public static FunctionExpression FunctionCall(string name, params Expression[] arguments)
        {
            return FunctionCall(name, arguments);
        }
        public static FunctionCallExpression FunctionCall(string name)
        {
            return new FunctionCallExpression(name, ReadOnlyCollection<Expression>.Empty);
        }

        public static SizeOfExpression SizeOf(Expression expression)
        {
            return new SizeOfExpression(expression);
        }

        public static ReturnExpression Return(IEnumerable<Expression> values)
        {
            return new ReturnExpression(new ReadOnlyCollection<Expression>(values.ToArray()));
        }

        public static BatchAssignExpression Assign(
            IEnumerable<Expression> parameters, params Expression[] values)
        {
            return new BatchAssignExpression(new ReadOnlyCollection<Expression>(parameters.ToArray()),
                new ReadOnlyCollection<Expression>(values.ToArray()));
        }

        public static NewMapExpression NewMap(uint count, uint dictCount)
        {
            return new NewMapExpression(count, dictCount);
        }

        public static CloneExpression Clone(Expression value)
        {
            return new CloneExpression(value);
        }

        public static FieldExpression Field(Expression instance, Expression fieldName)
        {
            return new FieldExpression(instance, fieldName);
        }
    }
}
