using System.IO;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.IO
{
    public sealed class EmptyStream : Stream, IReadOnlyStream
    {
        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => 0;

        public override long Position { get => 0; set => Seek(0, SeekOrigin.Begin); }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0;
        }

        public override void SetLength(long value)
        {
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
        }
    }
}
