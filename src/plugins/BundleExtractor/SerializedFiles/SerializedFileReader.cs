using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.SerializedFiles
{
    public class SerializedFileReader : IArchiveReader
    {
        public SerializedFileReader(EndianReader reader, IArchiveOptions? options)
        {
            _reader = reader;
            _options = options;
            _header.Read(reader);
            if (SerializedFileMetadata.IsMetadataAtTheEnd(_header.Version))
            {
                reader.BaseStream.Position = _header.FileSize - _header.MetadataSize;
            }
            SerializedFileMetadata metadata = new();
            metadata.Read(reader.BaseStream, _header);
        }

        private IArchiveOptions? _options;
        private EndianReader _reader;
        private SerializedFileHeader _header = new();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            throw new NotImplementedException();
        }

        public void ExtractToDirectory(string folder, Action<double>? progressFn = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            throw new NotImplementedException();
        }

        public bool IsSupport()
        {
            
            return true;
        }
    }
}
