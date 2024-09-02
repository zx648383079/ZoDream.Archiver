using SharpCompress.Archives.Zip;
using SharpCompress.Readers;
using System.IO;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Compression.Tar
{
    public class TarArchiveWriter : IArchiveWriter
    {
        public TarArchiveWriter(Stream stream, IArchiveOptions options)
        {
            _writer = ZipArchive.Open(stream, CompressHelper.Convert(options));
        }

        private readonly ZipArchive _writer;

        public IReadOnlyEntry AddEntry(string name, string fullPath)
        {
            var info = new FileInfo(fullPath);
            using var fs = File.OpenRead(fullPath);
            var res = _writer.AddEntry(name, fs, info.Length, info.CreationTime);
            return CompressHelper.Convert(res);
        }

        public IReadOnlyEntry AddEntry(string name, Stream input)
        {
            var res = _writer.AddEntry(name, input);
            return CompressHelper.Convert(res);
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
