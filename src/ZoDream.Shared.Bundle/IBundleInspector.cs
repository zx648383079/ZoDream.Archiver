namespace ZoDream.Shared.Bundle
{
    public interface IBundleInspector
    {
        public bool Inspect(IBundleSource source, IBundleOptions options);
    }
}
