namespace ZoDream.Shared.Interfaces
{
    public interface IArchiveOptions
    {
        public bool LeaveStreamOpen { get; }
        public bool LookForHeader { get; }
        public string? Password { get;}
        public string? Dictionary { get; }
    }
}
