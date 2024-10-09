using System.IO;
using System.Text;
using ZoDream.Shared.Interfaces;

namespace ZoDream.ChmExtractor
{
    public class ChmScheme : IArchiveScheme
    {
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            throw new System.NotImplementedException();
        }

        public bool IsReadable(Stream stream)
        {
            var pos = stream.Position;
            var buffer = new byte[4];
            stream.Read(buffer, 0, buffer.Length);
            stream.Seek(pos, SeekOrigin.Begin);
            return Encoding.ASCII.GetString(buffer) == "ITSF";
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            if (!fileName.EndsWith(".chm") || !IsReadable(stream))
            {
                return null;
            }
            return new ChmReader(new BinaryReader(stream), options);
        }
    }
}
