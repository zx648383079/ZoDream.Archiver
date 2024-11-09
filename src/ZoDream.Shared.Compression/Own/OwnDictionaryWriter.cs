using System;
using System.IO;
using System.Linq;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Compression.Own
{
    /// <summary>
    /// 结构  [0b: dotIndex, endPadding][5b=32b]
    /// </summary>
    public class OwnDictionaryWriter : IArchiveWriter
    {
        public OwnDictionaryWriter(Stream stream, IArchiveOptions options)
        {
            BaseStream = stream;
            _options = options;
            if (BaseStream.Position == 0)
            {
                BaseStream.Seek(1, SeekOrigin.Begin);
            }
        }

        private readonly Stream BaseStream;
        private readonly IArchiveOptions _options;
        private readonly byte[] _buffer = new byte[32 * 30];
        private int _count;
        private int _dotIndex;

        public IReadOnlyEntry AddEntry(string name, string fullPath)
        {
            using var fs = File.OpenRead(fullPath);
            return AddEntry(name, fs);
        }

        private bool TryCheckDot()
        {
            if (_dotIndex > 0)
            {
                return false;
            }
            var i = Array.IndexOf(_buffer, (byte)'.');
            if (i < 0)
            {
                return false;
            }
            _dotIndex = i + 1;
            for (; i < _buffer.Length - 1; i++)
            {
                _buffer[i] = _buffer[i + 1];
            }
            _count--;
            return true;
        }

        public IReadOnlyEntry AddEntry(string name, Stream input)
        {
            var compressedLength = 0L;
            while (true)
            {
                var c = input.Read(_buffer, _count, _buffer.Length - _count);
                if (c == 0)
                {
                    break;
                }
                _count += c;
                if (TryCheckDot() || _count != _buffer.Length)
                {
                    continue;
                }
                var res = OwnDictionary.Convert(_buffer, _count);
                _count = 0;
                BaseStream.Write(res);
                compressedLength += res.Length;
            }
            return new ReadOnlyEntry(name, input.Length, compressedLength, false, null);
        }

        public void AddDirectory(string folder)
        {
            AddFile(Directory.GetFiles(folder, "*"));
        }

        public void AddFile(string[] items, Action<double>? progressFn = null, CancellationToken token = default)
        {
            var fileItems = items.Where(File.Exists).OrderBy(item => Path.GetFileNameWithoutExtension(item)).ToArray();
            if (fileItems.Length == 0)
            {
                return;
            }
            var i = 0D;
            foreach (var item in fileItems)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                AddEntry(item, item);
                progressFn?.Invoke(++i / fileItems.Length);
            }
        }

        public void Dispose()
        {
            if (_count > 0)
            {
                BaseStream.Write(OwnDictionary.Convert(_buffer, _count));
            }
            BaseStream.Seek(0, SeekOrigin.Begin);
            BaseStream.WriteByte(MergeByte(_dotIndex, 32 - _count % 32));
            if (_options?.LeaveStreamOpen == false)
            {
                BaseStream.Dispose();
            }
        }

        public static (int, int) SplitByte(byte val)
        {
            var count = (val & 0b11111000) >> 3;
            var index = val & 0b111;
            return (index, count);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">最大7</param>
        /// <param name="count">最大31</param>
        /// <returns></returns>
        public static byte MergeByte(int index, int count)
        {
            return (byte)((index & 0b111) + ((count & 0b11111) << 3));
        }
    }
}
