using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Compression.Own
{
    public class OwnArchiveReader(Stream stream, IOwnKey key) : IArchiveReader
    {
        public OwnArchiveReader(Stream stream, IArchiveOptions options)
            : this (stream, OwnArchiveScheme.CreateKey(options))
        {
            _options = options;
        }

        private readonly IArchiveOptions? _options;
        private OwnFileHeader _header = new();
        private bool _nextPadding = false;
        private readonly int _maxBufferLength = 4096;


        private long ReadLength()
        {
            var code = stream.ReadByte();
            while (code == 0)
            {
                // 跳过长度为0的乱字符
                _nextPadding = !_nextPadding;
                code = stream.ReadByte();
            }
            if (code <= 250)
            {
                return code;
            }
            if (code <= 252)
            {
                return stream.ReadByte() + code * (code - 250);
            }
            var len = code - 251;
            var buffer = new byte[len];
            stream.Read(buffer, 0, len);
            var res = 0L;
            for (var j = len - 2; j >= 0; j--)
            {
                res += (long)Math.Pow(code, j);
            }
            for (var i = 0; i < len; i++)
            {
                res += (long)(buffer[i] * Math.Pow(256, len - i - 1));
            }
            return res;
        }

        private int ReadBytes(byte[] buffer, int length)
        {
            var len = stream.Read(buffer, 0, length);
            if (len == 0)
            {
                return len;
            }
            for (var i = 0; i < len; i++)
            {
                var code = key.ReadByte();
                buffer[i] = (byte)OwnHelper.Clamp(
                    _nextPadding ? (buffer[i] - code) : (buffer[i] + code),
                    256);
            }
            return len;
        }

        public string ReadName()
        {
            if (!_header.WithName)
            {
                return string.Empty;
            }
            var length = ReadLength();
            var buffer = new byte[length];
            ReadBytes(buffer, buffer.Length);
            _nextPadding = !_nextPadding;
            return Encoding.UTF8.GetString(buffer);
        }
        private void ReadStream(Stream output)
        {
            var buffer = new byte[_maxBufferLength];
            var length = ReadLength();
            if (length < 0)
            {
                return;
            }
            var i = 0L;
            while (i < length)
            {
                var len = ReadBytes(buffer, (int)Math.Min(length - i, buffer.Length));
                if (len == 0)
                {
                    return;
                }
                output.Write(buffer, 0, len);
                i += len;
            }
            _nextPadding = !_nextPadding;
        }

        private void JumpPart()
        {
            JumpPart(ReadLength());
        }

        private void JumpPart(long length)
        {
            stream.Seek(length, SeekOrigin.Current);
            key.Seek(length, SeekOrigin.Current);
            _nextPadding = !_nextPadding;
        }

        public IEnumerable<string> ReadFile()
        {
            IsSupport();
            if (!_header.Multiple)
            {
                var fileName = ReadName();
                yield return fileName;
                yield break;
            }
            while (stream.Position < stream.Length)
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
            var dirname = Path.GetDirectoryName(file);
            if (!Directory.Exists(dirname))
            {
                Directory.CreateDirectory(dirname!);
            }
            using var fs = File.Create(file);
            ReadStream(fs);
            return fileName;
        }

        public IEnumerable<string> ReadFile(string folder)
        {
            IsSupport();
            if (!_header.Multiple)
            {
                var fileName = ReadToFile(folder);
                yield return fileName;
                yield break;
            }
            while (stream.Position < stream.Length)
            {
                var name = ReadToFile(folder);
                yield return name;
            }
        }

        public IEnumerable<string> ReadFile(string folder, params string[] items)
        {
            IsSupport();
            if (!_header.Multiple)
            {
                var fileName = ReadToFile(folder);
                yield return fileName;
                yield break;
            }
            while (stream.Position < stream.Length)
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

        public bool IsSupport()
        {
            stream.Seek(0, SeekOrigin.Begin);
            try
            {
                _header.Read(stream);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsSupport(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            try
            {
                new OwnFileHeader().Read(stream);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            IsSupport();
            if (!_header.Multiple)
            {
                var fileName = ReadName();
                yield return new ReadOnlyEntry(fileName, ReadLength());
                yield break;
            }
            while (stream.Position < stream.Length)
            {
                var name = ReadName();
                var length = ReadLength();
                JumpPart(length);
                yield return new ReadOnlyEntry(name, length);
            }
        }

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            IsSupport();
            if (!_header.Multiple)
            {
                ReadName();
                ReadStream(output);
                return;
            }
            while (stream.Position < stream.Length)
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
                progressFn?.Invoke((double)stream.Position / stream.Length);
            }
        }

        public void Dispose()
        {
            key.Dispose();
            if (_options?.LeaveStreamOpen == false)
            {
                stream.Dispose();
            }
        }
    }
}
