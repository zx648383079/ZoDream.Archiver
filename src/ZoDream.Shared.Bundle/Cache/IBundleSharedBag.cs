using System;
using System.Diagnostics.CodeAnalysis;

namespace ZoDream.Shared.Bundle
{
    /// <summary>
    /// 临时共享内容
    /// </summary>
    public interface IBundleSharedBag : IDisposable
    {
        public bool TryGet<T>(int ptr, [NotNullWhen(true)] out T? result);
        public bool TryAdd<T>(int ptr, T result);
        public bool Contains(int ptr);
        public T GetOrAdd<T>(int ptr, Func<T> cb);
        public void Clear();
    }
}
