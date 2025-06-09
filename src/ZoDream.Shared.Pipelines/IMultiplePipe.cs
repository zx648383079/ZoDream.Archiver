using System.Collections.Generic;

namespace ZoDream.Shared.Pipelines
{
    public interface IMultiplePipe<T> : IPipe
    {
        public IEnumerable<T> Handle(T stream);
    }
}