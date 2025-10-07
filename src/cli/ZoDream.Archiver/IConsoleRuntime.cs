namespace ZoDream.Archiver
{
    public interface IConsoleRuntime
    {
        public Task RunAsync(CancellationToken token = default);
    }
}
