using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.SerializedFiles
{
    public class SerializedFileReader : IArchiveReader, ISerializedFile
    {
        public SerializedFileReader(EndianReader reader, string fullPath, IArchiveOptions? options)
        {
            FullPath = fullPath;
            _reader = reader;
            _options = options;
            _header.Read(reader);
            if (SerializedFileMetadata.IsMetadataAtTheEnd(_header.Version))
            {
                reader.BaseStream.Position = _header.FileSize - _header.MetadataSize;
            }
            _metadata.Read(reader.BaseStream, _header);
            CombineFormats(_header.Version, _metadata);
            _dependencyItems.AddRange(_metadata.Externals);
            for (int i = 0; i < _metadata.Object.Length; i++)
            {
                ref ObjectInfo objectInfo = ref _metadata.Object[i];
                reader.BaseStream.Position = _header.DataOffset + objectInfo.ByteStart;
                byte[] objectData = new byte[objectInfo.ByteSize];
                reader.BaseStream.ReadExactly(objectData);
                objectInfo.ObjectData = objectData;
            }
        }

        private readonly IArchiveOptions? _options;
        private readonly EndianReader _reader;
        private readonly SerializedFileHeader _header = new();
        private readonly SerializedFileMetadata _metadata = new();
        private readonly Dictionary<long, UIObject> _childrenDict = [];
        private readonly List<FileIdentifier> _dependencyItems = [];

        public IBundleContainer? Container { get; set; }
        public string FullPath { get; private set; }
        public SerializedType[] TypeItems => _metadata.Types;
        public FormatVersion Version => _header.Version;
        public UnityVersion UnityVersion => _metadata.UnityVersion;
        public List<UIObject> Children { get; private set; } = [];

        public BuildTarget Platform => _metadata.TargetPlatform;

        public IEnumerable<string> Dependencies => _dependencyItems.Select(i => i.PathName);
        public IEnumerable<ObjectInfo> ObjectMetaItems => _metadata.Object;

        public UIObject? this[long pathID] => _childrenDict[pathID];

        public string GetDependency(int index)
        {
            return _dependencyItems[index].PathName;
        }

        public int AddDependency(string dependency)
        {
            _dependencyItems.Add(new()
            {
                PathName = dependency,
            });
            return _dependencyItems.Count - 1;
        }

        public int IndexOf(string dependency)
        {
            return _dependencyItems.FindIndex(x => x.PathName.Equals(dependency, StringComparison.OrdinalIgnoreCase));
        }

        public EndianReader Create(ObjectInfo info)
        {
            bool swapEndian = SerializedFileHeader.HasEndianess(_header.Version) ?
                _header.Endianess : _metadata.SwapEndianess;
            return new EndianReader(
                new PartialStream(_reader.BaseStream, _header.DataOffset + info.ByteStart, info.ByteSize),
                swapEndian ? EndianType.BigEndian : EndianType.LittleEndian);
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

        public void AddChild(UIObject obj)
        {
            Children.Add(obj);
            _childrenDict.Add(obj.FileID, obj);
        }

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
