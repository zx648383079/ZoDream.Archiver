using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using ZoDream.Archiver.Dialogs;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Logging;

namespace ZoDream.Archiver.ViewModels
{
    public class EntryService : IEntryService, IDisposable
    {
        public EntryService(
            ILogger logger,
            ITemporaryStorage storage)
        {
            Add(logger);
            Add(storage);
        }
        public EntryService(ILogger logger)
            : this(logger, new TemporaryStorage())
        {
            
        }

        private readonly ConcurrentDictionary<string, object> _instanceItems = [];

        public void Add<T>(T instance)
        {
            Add(typeof(T).Name, instance);
        }

        public void Add(string key, object? instance)
        {
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

        public void AddIf<T>()
        {
            var key = typeof(T).Name;
            if (_instanceItems.ContainsKey(key))
            {
                return;
            }
            Add(key);
        }

        private bool Has(Type type)
        {
            if (type == typeof(IEntryService))
            {
                return true;
            }
            return _instanceItems.ContainsKey(type.Name);
        }

        public T Get<T>()
        {
            var type = typeof(T);
            return (T)Get(type.Name, type);
        }


        public T Get<T>(string key)
        {
            return (T)Get(key, typeof(T));
        }

        public object Get(Type type)
        {
            return Get(type.Name, type);
        }
        private object Get(string key, Type type)
        {
            if (type == typeof(IEntryService))
            {
                return this;
            }
            if (_instanceItems.TryGetValue(key, out var val))
            {
                return val;
            }
            foreach (var ctor in type.GetConstructors())
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

        public bool TryGet<T>([NotNullWhen(true)] out T? instance)
        {
            var type = typeof(T);
            if (!Has(type))
            {
                instance = default;
                return false;
            }
            instance = (T)Get(type);
            return true;
        }

        public bool TryGet<T>(string key, [NotNullWhen(true)] out T? instance)
        {
            if (!_instanceItems.TryGetValue(key, out var res))
            {
                instance = default;
                return false;
            }
            instance = (T)res;
            return true;
        }


        private ContentDialog CreateDialog(Type type)
        {
            if (typeof(IBundleOptions).IsAssignableFrom(type))
            {
                return new BundleDialog();
            }
            return new PasswordDialog();
        }

        public async Task<T?> AskAsync<T>()
        {
            var type = typeof(T);
            if (!type.IsClass)
            {
                return default;
            }
            var picker = CreateDialog(type);
            var model = picker.DataContext as IEntryConfiguration;
            var instance = Activator.CreateInstance(type);
            model!.Load(this, instance!);
            var res = await App.ViewModel.OpenFormAsync(picker);
            if (!res)
            {
                return default;
            }
            model.Unload(this, instance!);
            return (T)instance;
        }

        public bool CheckPoint(int hashCode)
        {
            return TryLoadPoint(hashCode, out _);
        }
        public void SavePoint(int hashCode, uint record)
        {
            App.ViewModel.Setting.Set($"_h_{hashCode}", record);
        }

        public bool TryLoadPoint(int hashCode, out uint record)
        {
            record = App.ViewModel.Setting.Get<uint>($"_h_{hashCode}");
            return record > 0;
        }

        public void Dispose()
        {
            foreach (var item in _instanceItems)
            {
                if (item.Value is IDisposable c)
                {
                    c.Dispose();
                    _instanceItems.TryRemove(item);
                }
            }
        }
    }
}
