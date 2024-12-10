using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Shared.Language.AST
{
    public class FieldExpression: Expression
    {

        public Expression Instance { get; }
        public Expression Field { get; }

        public FieldExpression(Expression instance, Expression fieldName)
        {
            Instance = instance; 
            Field = fieldName;
        }
    }
}
