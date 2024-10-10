namespace ZoDream.Shared.Interfaces
{
    public interface IWriteOnlyStream
    {
        public void Write(byte[] buffer, int offset, int count);
    }
}
