using System;
using System.Collections.Concurrent;
using System.Linq;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme : IBundleService, IDisposable
    {
        public BundleScheme(ILogger logger)
        {
            Add(logger);
            Add<ITemporaryStorage>(new TemporaryStorage());
            Initialize();
        }

        private readonly ConcurrentDictionary<Type, object> _instanceItems = [];

        public void Add<T>(T instance)
        {
            Add(typeof(T), instance);
        }

        public void AddIf<T>()
        {
            var key = typeof(T);
            if (_instanceItems.ContainsKey(key))
            {
                return;
            }
            Add(key);
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
            if (type == typeof(IBundleService)
                || type == typeof(IBundleScheme)
                || type == typeof(BundleScheme))
            {
                return true;
            }
            return _instanceItems.ContainsKey(type);
        }

        private object Get(Type type)
        {
            if (type == typeof(IBundleService) 
                || type == typeof(IBundleScheme)
                || type == typeof(BundleScheme))
            {
                return this;
            }
            var key = type;
            if (_instanceItems.TryGetValue(key, out var val))
            {
                return val;
            }
            foreach (var ctor in key.GetConstructors())
            {
                if (!ctor.IsPublic)
                {
                    continue;
                }
                var parameters = ctor.GetParameters();
                if (ctor.GetParameters().Length == 0)
                {
                    return ctor.Invoke(null);
                }
                if (parameters.Where(i => !Has(i.ParameterType)).Any())
                {
                    continue;
                }
                return ctor.Invoke(parameters.Select(i => Get(i.ParameterType)).ToArray()); ;
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
