using System;

namespace ZoDream.Shared.Net
{
    public class Packet(long position, byte[] data, int length) : IDisposable
    {
        public Memory<byte> Data { get; private set; } = data.AsMemory(0, length);
        public int Length => length;
        public long Position { get; private set; } = position;
        public long EndPosition => Position + Length;

        public void Dispose()
        {
            Data = null;
            Position = 0;
        }
    }
}
