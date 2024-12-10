using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Shared.Language.AST
{
    public class NewMapExpression: Expression
    {
        public uint Count { get; }

        public uint DictCount { get; }


        internal NewMapExpression(uint count, uint dictCount)
        {
            Count = count;
            DictCount = dictCount;
        }
    }
}
