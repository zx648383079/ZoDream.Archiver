using SharpCompress.Compressors.Xz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

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
            CombineFormats(_header.Version, metadata);
            for (int i = 0; i < metadata.Object.Length; i++)
            {
                ref ObjectInfo objectInfo = ref metadata.Object[i];
                reader.BaseStream.Position = _header.DataOffset + objectInfo.ByteStart;
                byte[] objectData = new byte[objectInfo.ByteSize];
                reader.BaseStream.ReadExactly(objectData);
                objectInfo.ObjectData = objectData;
            }

            SetProperties(_header, metadata);
        }

        private static void CombineFormats(FormatVersion generation, SerializedFileMetadata origin)
        {
            if (!SerializedFileMetadata.HasEnableTypeTree(generation))
            {
                origin.EnableTypeTree = true;
            }
            if (generation >= FormatVersion.RefactorTypeData)
            {
                for (int i = 0; i < origin.Object.Length; i++)
                {
                    origin.Object[i].Initialize(origin.Types);
                }
            }
        }

        private void SetProperties(SerializedFileHeader header, SerializedFileMetadata metadata)
        {
            //Generation = header.Version;
            //Version = metadata.UnityVersion;
            //Platform = metadata.TargetPlatform;
            //EndianType = GetEndianType(header, metadata);
            //m_dependencies = metadata.Externals;
            //m_objects = metadata.Object;
            //m_types = metadata.Types;
            //m_scriptTypes = metadata.ScriptTypes;
            //m_refTypes = metadata.RefTypes;
            //HasTypeTree = metadata.EnableTypeTree;
        }

        private IArchiveOptions? _options;
        private EndianReader _reader;
        private SerializedFileHeader _header = new();

        public void Dispose()
        {
            if (_options?.LeaveStreamOpen == false)
            {
                _reader.Dispose();
            }
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
