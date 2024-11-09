using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.Shared.Compression.Own
{
    /// <summary>
    /// 结构  [0b: dotIndex, endPadding][5b=32b]
    /// </summary>
    public class OwnDictionaryReader : IArchiveReader
    {
        public OwnDictionaryReader(Stream stream, IArchiveOptions options)
        {
            BaseStream = stream;
            _options = options;
            var pos = BaseStream.Position;
            BaseStream.Seek(0, SeekOrigin.Begin);
            var b = BaseStream.ReadByte();
            if (b >= 0)
            {
                (_dotIndex, _padding) = OwnDictionaryWriter.SplitByte((byte)b);
            }
            if (pos > 1)
            {
                BaseStream.Seek(pos, SeekOrigin.Begin);
            }
            _length = (BaseStream.Length - 1) * 32 / 5 - _padding;
        }

        private readonly Stream BaseStream;
        private readonly IArchiveOptions _options;
        private readonly int _padding;
        private readonly int _dotIndex;
        private readonly long _length;

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            return [
                new ReadOnlyEntry("undefined.txt", _length)
            ];
        }

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            ExtractTo(output);
        }

        private void ExtractTo(Stream output, Action<double>? progressFn = null, CancellationToken token = default)
        {
            var buffer = new byte[5 * 100];
            var pos = (BaseStream.Position - 1) * 32 / 5;
            var dotPos = _dotIndex - 1;
            while (!token.IsCancellationRequested)
            {
                var c = BaseStream.Read(buffer, 0, buffer.Length);
                if (c == 0)
                {
                    break;
                }
                var res = OwnDictionary.ConvertBack(buffer, c);
                if (pos <= dotPos && pos + res.Length > dotPos)
                {
                    var offset = (int)(dotPos - pos);
                    if (offset > 0)
                    {
                        BaseStream.Write(res, 0, offset);
                    }
                    BaseStream.WriteByte((byte)'.');
                    BaseStream.Write(res, offset, res.Length - offset);
                } else
                {
                    BaseStream.Write(res);
                }
                pos += res.Length;
                progressFn?.Invoke(pos / _length);
            }
            if (_padding > 0)
            {
                output.SetLength(output.Position - _padding);
                output.Flush();
            }
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            if (!LocationStorage.TryCreate(Path.Combine(folder, "undefined.txt"), mode, out var fileName))
            {
                return;
            }
            using var fs = File.Create(fileName);
            ExtractTo(fs, progressFn, token);
        }

        public void Dispose()
        {
            if (_options?.LeaveStreamOpen == false)
            {
                BaseStream.Dispose();
            }
        }


    }
}
