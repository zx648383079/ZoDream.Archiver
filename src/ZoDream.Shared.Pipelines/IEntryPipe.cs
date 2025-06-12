using System.Collections.Generic;

namespace ZoDream.Shared.Pipelines
{
    /// <summary>
    /// 适用于自定义入口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEntryPipe<T> : IPipe
    {
        public IEnumerable<T> Handle();
    }

    /// <summary>
    /// 适用于结束
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEndPipe<T> : IPipe
    {
        public void Handle(IEnumerable<T> request);
    }
}
