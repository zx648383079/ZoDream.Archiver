namespace ZoDream.Shared.Net
{
    public delegate void RequestChangedEventHandler(RequestChangedEventArgs args);
    public delegate void RequestProgressEventHandler(long received);

    public struct RequestChangedEventArgs
    {
        public string FileName;
        public string OutputPath;
        public long Length;
        public RequestStatus Status;
    }
}
