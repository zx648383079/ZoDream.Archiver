namespace ZoDream.Shared.Interfaces
{
    public interface IReadOnlyStream
    {
        public int Read(byte[] buffer, int offset, int count);
    }
}
