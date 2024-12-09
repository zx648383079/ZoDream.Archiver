using System;
using System.IO;
using System.Text;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Compression.Own
{
    public class OwnArchiveWriter : IArchiveWriter
    {

        public OwnArchiveWriter(Stream stream, IOwnKey key, OwnFileHeader header)
        {
            BaseStream = stream;
            _key = key;
            _header = header;
            _header.Write(stream);
            _compressor = _header.Version switch
            {
                OwnVersion.V3 => new V3.OwnArchiveCompressor(key),
                OwnVersion.V2 => new V2.OwnArchiveCompressor(key),
                _ => new OwnArchiveCompressor(key)
            };
        }

        public OwnArchiveWriter(Stream stream, IArchiveOptions options)
            : this(stream, OwnArchiveScheme.CreateKey(options), new OwnFileHeader(true))
        {
            _options = options;
        }

        private readonly IOwnKey _key;
        private readonly OwnFileHeader _header;
        private readonly Stream BaseStream;
        private readonly IOwnArchiveCompressor _compressor;
        private readonly IArchiveOptions? _options;
        private bool _nextPadding = false;

        private void WriteName(string fileName)
        {
            var buffer = Encoding.UTF8.GetBytes(fileName);
            WriteLength(buffer.Length);
            using var ms = new MemoryStream(buffer);
            var inflator = _compressor.CreateInflator(ms, buffer.Length, _nextPadding);
            inflator.CopyTo(BaseStream);
            _nextPadding = !_nextPadding;
        }

        private void WriteLength(long length)
        {
            OwnHelper.WriteLength(BaseStream, length);
        }


        private void WriteStream(Stream input)
        {
            var length = input.Length - input.Position;
            WriteLength(length);
            var inflator = _compressor.CreateInflator(input, length, _nextPadding);
            inflator.CopyTo(BaseStream, length);
            _nextPadding = !_nextPadding;
        }

        public IReadOnlyEntry AddEntry(string name, string fullPath)
        {
            using var fs = File.OpenRead(fullPath);
            return AddEntry(name, fs);
        }

        public IReadOnlyEntry AddEntry(string name, Stream input)
        {
            if (_header.WithName)
            {
                WriteName(name);
            }
            WriteStream(input);
            return new ReadOnlyEntry(name, input.Length);
        }

        public void Dispose()
        {
            _key.Dispose();
            if (_options?.LeaveStreamOpen == false)
            {
                BaseStream.Dispose();
            }
        }
    }
}
