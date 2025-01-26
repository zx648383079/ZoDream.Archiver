using System.Linq.Expressions;

namespace ZoDream.Shared.Language.AST
{
    public class IdentifierExpression : Expression
    {

        public string Value { get; }

        public IdentifierExpression(string name)
        {
            Value = name;
        }
    }
}
