using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;
using Object = UnityEngine.Object;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.SerializedFiles
{
    internal partial class SerializedFileReader : IArchiveReader
    {
        public SerializedFileReader(IBundleBinaryReader reader, 
            IFilePath sourcePath, 
            IArchiveOptions? options)
        {
            FullPath = sourcePath;
            _reader = reader;
            _options = options;
            _header.Read(reader);
            if (SerializedFileMetadata.IsMetadataAtTheEnd(_header.Version))
            {
                reader.BaseStream.Position = Math.Max(reader.BaseStream.Position, _header.FileSize - _header.MetadataSize);
            }
            if (reader.TryGet<Version>(out var version))
            {
                _metadata.Version = version;
            }
            _metadata.Read(reader.BaseStream, _header);
            CombineFormats(_header.Version, _metadata);
            foreach (var item in _metadata.Externals)
            {
                if (item.PathNameOrigin.StartsWith("Library/"))
                {
                    AddDependency(new FilePathName(item.PathName));
                } else if (item.PathNameOrigin.StartsWith("archive:/"))
                {
                    AddDependency(new EntryName(item.PathName));
                }
            }
            _children = new Object?[_metadata.Object.Length];
            _objectIdMap = ImmutableDictionary.CreateRange(_metadata.Object.Select((item, i) => new KeyValuePair<long, int>(item.FileID, i)));
        }

        private readonly IArchiveOptions? _options;
        private readonly IBundleBinaryReader _reader;
        private readonly SerializedFileHeader _header = new();
        private readonly SerializedFileMetadata _metadata = new();
        
        private static void CombineFormats(FormatVersion generation, SerializedFileMetadata origin)
        {
            if (!SerializedFileMetadata.HasEnableTypeTree(generation))
            {
                origin.EnableTypeTree = true;
            }
            if (generation >= FormatVersion.RefactorTypeData)
            {
                for (var i = 0; i < origin.Object.Length; i++)
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


        public void Dispose()
        {
            if (_options?.LeaveStreamOpen == false)
            {
                _reader.LeaveStreamOpen = false;
                _reader.Dispose();
            }
        }

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            throw new NotImplementedException();
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            throw new NotImplementedException();
        }

    }
}
