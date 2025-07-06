using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace ZoDream.Shared.Bundle
{
    public class BundleSharedBag : IBundleSharedBag
    {
        private readonly ConcurrentDictionary<int, object> _items = [];

        public bool Contains(int ptr)
        {
            return _items.ContainsKey(ptr);
        }

        public T GetOrAdd<T>(int ptr, Func<T> cb)
        {
            if (TryGet<T>(ptr, out var res))
            {
                return res;
            }
            _items.TryAdd(ptr, res = cb.Invoke());
            return res;
        }

        public bool TryAdd<T>(int ptr, T result)
        {
            if (_items.TryAdd(ptr, result))
            {
                return true;
            }
            if (result is null)
            {
                return false;
            }
            if (_items[ptr] != (object)result && _items[ptr] is IDisposable d)
            {
                d.Dispose();
            }
            _items[ptr] = result;
            return false;
        }

        public bool TryGet<T>(int ptr, [NotNullWhen(true)] out T? result)
        {
            if (_items.TryGetValue(ptr, out var res))
            {
                result = (T)res;
                return true;
            }
            result = default;
            return false;
        }

        public void Clear()
        {
            foreach (var item in _items)
            {
                if (item.Value is IDisposable d)
                {
                    d.Dispose();
                }
            }
            _items.Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
