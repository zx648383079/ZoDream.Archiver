namespace ZoDream.Shared.Bundle
{
    public delegate void BundleChangedEventHandler(BundleChangedEventArgs args);
    public delegate void BundleProgressEventHandler(long received);

    public struct BundleChangedEventArgs
    {
        public string Name;
        public string OutputPath;
        public long Length;
        public BundleStatus Status;
    }
}
