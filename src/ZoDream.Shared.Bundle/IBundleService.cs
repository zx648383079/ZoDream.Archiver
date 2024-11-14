namespace ZoDream.Shared.Bundle
{
    public interface IBundleService
    {
        public void Add<T>(T instance);
        public T? Get<T>();
    }
}
