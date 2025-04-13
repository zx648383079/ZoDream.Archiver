using System;
using System.Threading.Tasks;

namespace ZoDream.Shared.Interfaces
{
    public interface ISettingContainer
    {
        public Task LoadAsync();

        public T Get<T>(object key);
        public T Get<T>(object key, T def);
        public void Set<T>(object key, T value);

        public void Delete(object key);
        public bool Exist(object key);

        public T Get<T>(string key);
        public T Get<T>(string key, T def);
        public void Set<T>(string key, T value);

        public void Delete(string key);
        public bool Exist(string key);

        public Task SaveAsync();

        public void Delete(Func<string, bool> whereKey);
    }
}
