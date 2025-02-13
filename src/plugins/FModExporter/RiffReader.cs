using FMOD;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.FModExporter
{
    public class RiffReader(Stream input, IArchiveOptions? options) : IArchiveReader
    {
        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            var finder = new StreamFinder("FSB5")
            {
                IsMatchFirst = true
            };
            var reader = new BinaryReader(input);
            var i = 0;
            while (input.Position < input.Length && finder.MatchFile(input))
            {
                var offset = finder.BeginPosition.First();
                input.Position = offset + 4;
                var version = reader.ReadUInt32();
                var numSamples = reader.ReadUInt32();
                var shdrSize = reader.ReadUInt32();
                var nameSize = reader.ReadUInt32();
                var dataSize = reader.ReadUInt32();
                var size = 0x3c + shdrSize + nameSize + dataSize;
                if (offset + size > input.Length)
                {
                    continue;
                }
                var name = $"{++i}";
                //if (nameSize > 0)
                //{
                //    input.Position = offset + 0x3c + shdrSize;
                //    name = Encoding.ASCII.GetString(input.ReadBytes((int)nameSize));
                //}
                yield return new ArchiveEntry(name + ".fsb", offset, size);
                input.Position = offset + size;
            }
        }

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            throw new NotImplementedException();
        }


        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            var items = ReadEntry().ToArray();
            var i = 0;
            foreach (var item in items)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                new FModReader(new PartialStream(input, ((ArchiveEntry)item).Offset, item.Length), item.Name, options)
                    .ExtractToDirectory(folder, mode, null, token);
                progressFn?.Invoke((double)(++i) / items.Length);
            }
        }

        

        public void Dispose()
        {
            if (options?.LeaveStreamOpen != true)
            {
                input.Dispose();
            }
        }

        public static bool IsSupport(Stream input)
        {
            var pos = input.Position;
            try
            {
                if (!input.ReadBytes(4).Equal("RIFF"))
                {
                    return false;
                }
                input.Seek(4, SeekOrigin.Current);
                return input.ReadBytes(4).Equal("FEV ");
            } 
            finally
            {
                input.Position = pos;
            }
        }
    }
}
