using System.Collections.Generic;
using System.Linq;

namespace ZoDream.Shared.Pipelines
{
    public abstract class PipelineBehavior : IPipelineBehavior
    {

        public static IEnumerable<T> Handle<T>(T input, IEnumerable<IPipe> pipes)
        {
            T[] next = [input];
            foreach (var item in pipes)
            {
                switch (item)
                {
                    case IPipe<T> a:
                        next = [.. next.Select(a.Handle)];
                        break;
                    case IMultiplePipe<T> b:
                        next = [.. next.SelectMany(b.Handle)];
                        break;
                }
            }
            return next;
        }
    }
}
