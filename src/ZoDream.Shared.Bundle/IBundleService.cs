namespace ZoDream.Shared.Bundle
{
    public interface IBundleService
    {
        public void Add<T>(T instance);
        public void AddIf<T>();
        public T Get<T>();
    }
}
