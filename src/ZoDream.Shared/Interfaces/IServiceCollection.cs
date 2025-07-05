using System;
using System.Diagnostics.CodeAnalysis;

namespace ZoDream.Shared.Interfaces
{
    public interface IServiceCollection : IDisposable
    {
        public void Add<T>(T instance);
        public void Add(string key, object? instance);
        public void AddIf<T>();
        public void AddIf<T>(T? instance);
        public T Get<T>();
        public object Get(Type type);
        public T Get<T>(string key);

        public bool TryGet<T>([NotNullWhen(true)] out T? instance);
        public bool TryGet<T>(string key, [NotNullWhen(true)] out T? instance);
    }
}
