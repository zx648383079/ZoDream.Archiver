using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace ZoDream.Shared.Language.AST
{
    public class FunctionExpression : Expression
    {

        public string Name { get; }

        public ReadOnlyCollection<ParameterExpression> Parameters { get; }
        public Expression Body { get; }

        public Type ReturnType { get; }

        internal FunctionExpression(string name, ReadOnlyCollection<ParameterExpression> parameters, Expression body, Type returnType)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
            ReturnType = returnType;
        }
    }
}
