using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Compression.Own
{
    public class OwnArchiveReader : IArchiveReader
    {

        public OwnArchiveReader(Stream stream, IOwnKey key)
        {
            _key = key;
            BaseStream = stream;
            _header.Read(stream);
            _compressor = _header.Version switch
            {
                OwnVersion.V3 => new V3.OwnArchiveCompressor(key),
                OwnVersion.V2 => new V2.OwnArchiveCompressor(key),
                _ => new OwnArchiveCompressor(key)
            };
        }

        public OwnArchiveReader(Stream stream, IArchiveOptions options)
            : this (stream, OwnArchiveScheme.CreateKey(options))
        {
            _options = options;
        }

        private readonly IOwnKey _key;
        private readonly Stream BaseStream;
        private readonly IArchiveOptions? _options;
        private readonly OwnFileHeader _header = new();
        private readonly IOwnArchiveCompressor _compressor;
        private bool _nextPadding = false;


        private long ReadLength()
        {
            while (true)
            {
                var length = OwnHelper.ReadLength(BaseStream);
                if (length != 0)
                {
                    return length;
                }
                // 跳过长度为0的乱字符
                _nextPadding = !_nextPadding;
            }
        }

        public string ReadName()
        {
            if (!_header.WithName)
            {
                return string.Empty;
            }
            var length = ReadLength();
            if (length < 0)
            {
                return string.Empty;
            }
            var buffer = new byte[length];
            var deflator = _compressor.CreateDeflator(BaseStream, length, _nextPadding);
            deflator.ReadExactly(buffer);
            _nextPadding = !_nextPadding;
            return Encoding.UTF8.GetString(buffer);
        }
        private void ReadStream(Stream output)
        {
            var length = ReadLength();
            if (length < 0)
            {
                return;
            }
            var deflator = _compressor.CreateDeflator(BaseStream, length, _nextPadding);
            deflator.CopyTo(output, length);
            _nextPadding = !_nextPadding;
        }

        private void JumpPart()
        {
            JumpPart(ReadLength());
        }

        private void JumpPart(long length)
        {
            BaseStream.Seek(length, SeekOrigin.Current);
            _key.Seek(length, SeekOrigin.Current);
            _nextPadding = !_nextPadding;
        }

        public IEnumerable<string> ReadFile()
        {
            if (!_header.Multiple)
            {
                var fileName = ReadName();
                yield return fileName;
                yield break;
            }
            while (BaseStream.Position < BaseStream.Length)
            {
                var name = ReadName();
                JumpPart();
                yield return name;
            }
        }

        private string ReadToFile(string folder)
        {
            return ReadToFile(folder, ReadName());
        }

        private string ReadToFile(string folder, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = OwnHelper.RandomName(folder);
            }
            var file = Path.Combine(folder, OwnHelper.GetSafePath(fileName));
            var folderName = Path.GetDirectoryName(file);
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName!);
            }
            using var fs = File.Create(file);
            ReadStream(fs);
            return fileName;
        }

        public IEnumerable<string> ReadFile(string folder)
        {
            if (!_header.Multiple)
            {
                var fileName = ReadToFile(folder);
                yield return fileName;
                yield break;
            }
            while (BaseStream.Position < BaseStream.Length)
            {
                var name = ReadToFile(folder);
                yield return name;
            }
        }

        public IEnumerable<string> ReadFile(string folder, params string[] items)
        {
            if (!_header.Multiple)
            {
                var fileName = ReadToFile(folder);
                yield return fileName;
                yield break;
            }
            while (BaseStream.Position < BaseStream.Length)
            {
                var name = ReadName();
                if (!items.Contains(name))
                {
                    JumpPart();
                    continue;
                }
                name = ReadToFile(folder, name);
                yield return name;
            }
        }


        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            if (!_header.Multiple)
            {
                var fileName = ReadName();
                yield return new ReadOnlyEntry(fileName, ReadLength());
                yield break;
            }
            while (BaseStream.Position < BaseStream.Length)
            {
                var name = ReadName();
                var length = ReadLength();
                JumpPart(length);
                yield return new ReadOnlyEntry(name, length);
            }
        }

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            if (!_header.Multiple)
            {
                ReadName();
                ReadStream(output);
                return;
            }
            while (BaseStream.Position < BaseStream.Length)
            {
                var name = ReadName();
                if (entry.Name != name)
                {
                    JumpPart();
                    continue;
                }
                ReadStream(output);
                return;
            }
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            foreach (var _ in ReadFile(folder))
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                progressFn?.Invoke((double)BaseStream.Position / BaseStream.Length);
            }
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
