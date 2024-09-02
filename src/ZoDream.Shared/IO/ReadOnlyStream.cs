using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.IO
{
    public abstract class ReadOnlyStream(Stream stream): Stream, IReadOnlyStream
    {
        public override bool CanRead => stream.CanRead;

        public override bool CanSeek => stream.CanSeek;

        public override bool CanWrite => false;

        public override long Length => stream.Length;

        public override long Position { 
            get => stream.Position; 
            set {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override void Flush()
        {
        }


        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
