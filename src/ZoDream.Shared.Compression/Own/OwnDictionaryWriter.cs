using System;
using System.IO;
using System.Linq;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Compression.Own
{
    public class OwnDictionaryWriter(Stream stream, IArchiveOptions options) : IArchiveWriter
    {
        public IReadOnlyEntry AddEntry(string name, string fullPath)
        {
            using var fs = File.OpenRead(fullPath);
            return AddEntry(name, fs);
        }

        public IReadOnlyEntry AddEntry(string name, Stream input)
        {
            var buffer = new byte[32 * 30];
            var compressedLength = 0L;
            while (true)
            {
                var c = input.Read(buffer, 0, buffer.Length);
                if (c == 0)
                {
                    break;
                }
                var res = OwnDictionary.Convert(buffer, c);
                stream.Write(res);
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
            if (options?.LeaveStreamOpen == false)
            {
                stream.Dispose();
            }
        }
    }
}
