using System;
using System.IO;

namespace ZoDream.Shared.IO
{
    public static class StreamExtension
    {
        public static long CopyTo(this Stream input, Stream output, long length)
        {
            var buffer = new byte[Math.Min(length, 1024 * 5)];
            var len = 0L;
            while (len < length)
            {
                var res = input.Read(buffer, 0, buffer.Length);
                if (res == 0)
                {
                    break;
                }
                output.Write(buffer, 0, res);
                len += res;
            }
            return len;
        }
    }
}
