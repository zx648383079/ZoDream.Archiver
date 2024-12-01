namespace ZoDream.Shared.Bundle
{
    public interface IBundleElementScanner
    {
        public bool TryRead(IBundleBinaryReader reader, object instance);
    }
}
