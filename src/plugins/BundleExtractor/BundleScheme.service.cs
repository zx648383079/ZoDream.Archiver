using System;
using System.Collections.Concurrent;
using System.Linq;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme : IBundleService, IDisposable
    {
        public BundleScheme(ILogger logger)
        {
            Add(logger);
        }

        private readonly ConcurrentDictionary<Type, object> _instanceItems = [];

        public void Add<T>(T instance)
        {
            Add(typeof(T), instance);
        }

        public T Get<T>()
        {
            return (T)Get(typeof(T));
        }

        private void Add(Type type, object? instance)
        {
            var key = type;
            if (!_instanceItems.TryGetValue(key, out var val))
            {
                _instanceItems.TryAdd(key, instance);
                return;
            }
            if (val == instance)
            {
                return;
            }
            if (val is IDisposable c)
            {
                c.Dispose();
            }
            _instanceItems[key] = instance;
        }

        private bool Has(Type type)
        {
            return _instanceItems.ContainsKey(type);
        }

        private object Get(Type type)
        {
            var key = type;
            if (_instanceItems.TryGetValue(key, out var val))
            {
                return val;
            }
            object obj;
            foreach (var ctor in key.GetConstructors())
            {
                if (!ctor.IsPublic)
                {
                    continue;
                }
                var parameters = ctor.GetParameters();
                if (ctor.GetParameters().Length == 0)
                {
                    obj = ctor.Invoke(null);
                    Add(type, obj);
                    return obj;
                }
                if (parameters.Where(i => !Has(i.ParameterType)).Any())
                {
                    continue;
                }
                obj = ctor.Invoke(parameters.Select(i => Get(i.ParameterType)).ToArray());
                Add(type, obj);
                return obj;
            }
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            foreach (var item in _instanceItems)
            {
                if (item.Value is IDisposable c)
                {
                    c.Dispose();
                }
            }
        }
    }
}
