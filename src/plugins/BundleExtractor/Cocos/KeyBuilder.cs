using System.IO;
using System.Text;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Cocos
{
    public class KeyBuilder
    {
        private readonly byte[] _a = Encoding.ASCII.GetBytes("FnJL7/EDzxyWj/caY9");
        private readonly byte[] _b = Encoding.ASCII.GetBytes("JNDYdpyRCeDuHO");
        private readonly byte[] _c = Encoding.ASCII.GetBytes("pyRCeD");
        private readonly byte[] _d = Encoding.ASCII.GetBytes("J/EDzxyWj/z");

        public byte[] XorKey { get; private set; } = [];
        public byte[] XXTeaKey { get; private set; } = [];

        public void Update(string fileName)
        {
            if (fileName.EndsWith(".lua"))
            {
                XorKey = [_d[^1], .._a, .._d[..^1]];
                XXTeaKey = [.._c, .._b[..^4]];
            }
            else
            {
                var data = Encoding.ASCII.GetBytes(fileName);
                XorKey = [.._a[10..16], .. data[^2..], .. _a[..10]];
                XXTeaKey = [.. data[..2], .. _b];
            }
        }
        public Stream Convert(Stream input)
        {
            return new PartialStream(input, 10, input.Length - 10);
        }
    }
}
