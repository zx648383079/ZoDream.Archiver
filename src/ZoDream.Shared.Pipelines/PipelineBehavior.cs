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
                        next = [.. next.Select(a.Handle).Where(i => i is not null)];
                        break;
                    case IMultiplePipe<T> b:
                        next = [.. next.SelectMany(b.Handle).Where(i => i is not null)];
                        break;
                }
            }
            return next;
        }

        public static void Handle<T>(IEnumerable<IPipe> pipes)
        {
            T[] next = [];
            foreach (var item in pipes)
            {
                // TODO 需要考虑大量数据，切换成单任务执行完全部流程
                switch (item)
                {
                    case IEntryPipe<T> entry:
                        next = [.. entry.Handle()];
                        break;
                    case IEndPipe<T> end:
                        end.Handle(next);
                        next = [];
                        break;
                    case IPipe<T> a:
                        next = [.. next.Select(a.Handle).Where(i => i is not null)];
                        break;
                    case IMultiplePipe<T> b:
                        next = [.. next.SelectMany(b.Handle).Where(i => i is not null)];
                        break;
                }
            }
        }
    }
}
