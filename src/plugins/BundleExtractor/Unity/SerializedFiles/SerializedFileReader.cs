using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Document;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Logging;
using ZoDream.Shared.Models;
using Object = UnityEngine.Object;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.SerializedFiles
{
    internal class SerializedFileReader : IArchiveReader, ISerializedFile
    {
        public SerializedFileReader(IBundleBinaryReader reader, string fullPath, IArchiveOptions? options)
        {
            FullPath = fullPath;
            _reader = reader;
            _options = options;
            _header.Read(reader);
            if (SerializedFileMetadata.IsMetadataAtTheEnd(_header.Version))
            {
                reader.BaseStream.Position = Math.Max(reader.BaseStream.Position, _header.FileSize - _header.MetadataSize);
            }
            _metadata.Read(reader.BaseStream, _header);
            CombineFormats(_header.Version, _metadata);
            _dependencyItems.AddRange(_metadata.Externals);
            _children = new Object?[_metadata.Object.Length];
            _objectIdMap = ImmutableDictionary.CreateRange(_metadata.Object.Select((item, i) => new KeyValuePair<long, int>(item.FileID, i)));
        }

        private readonly IArchiveOptions? _options;
        private readonly IBundleBinaryReader _reader;
        private readonly SerializedFileHeader _header = new();
        private readonly SerializedFileMetadata _metadata = new();
        private readonly ImmutableDictionary<long, int> _objectIdMap;
        private readonly HashSet<int> _excludeItems = [];
        /// <summary>
        /// 跟 _metadata.Object 一一对应
        /// </summary>
        private readonly Object?[] _children;
        private readonly List<FileIdentifier> _dependencyItems = [];

        public IBundleContainer? Container { get; set; }

        public ILogger? Logger => Container?.Logger;
        public string FullPath { get; private set; }
        public FormatVersion Format => _header.Version;
        public Version Version => _metadata.Version;

        public BuildTarget Platform => _metadata.TargetPlatform;

        public int Count => _metadata.Object.Length;

        public IEnumerable<string> Dependencies => _dependencyItems.Select(i => i.PathName);

        public Object? this[int index] 
        {
            get => index >= 0 && index < Count ? _children[index] : null;
            set {
                if (index >= 0 && index < Count)
                {
                    _children[index] = value;
                }
            }
        }

        public int IndexOf(long pathID)
        {
            if (_objectIdMap.TryGetValue(pathID, out var index))
            {
                return index;
            }
            return -1;
        }

        public int IndexOf(Object obj)
        {
            if (obj is null)
            {
                return -1;
            }
            return Array.FindIndex(_children, item => item == obj);
        }

        public ObjectInfo Get(int index)
        {
            Debug.Assert(index >= 0 && index < Count);
            return _metadata.Object[index];
        }

        public VirtualDocument? GetType(int index)
        {
            Debug.Assert(index >= 0 && index < Count);
            var i = _metadata.Object[index].SerializedTypeIndex;
            return i < 0 ? null : _metadata.Types[i].OldType;
        }

        /// <summary>
        /// 添加一个不需要导出
        /// </summary>
        /// <param name="fileId"></param>
        public void AddExclude(long fileId)
        {
            var index = IndexOf(fileId);
            if (index < 0)
            {
                return;
            }
            _excludeItems.Add(index);
        }
        /// <summary>
        /// 判断一个对象不需要导出
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public bool IsExclude(long fileId)
        {
            var index = IndexOf(fileId);
            if (index < 0)
            {
                return false;
            }
            return _excludeItems.Contains(index);
        }

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

        public IBundleBinaryReader OpenRead(int index)
        {
            Debug.Assert(index >= 0 && index < Count);
            return OpenRead(_metadata.Object[index]);
        }

        public IBundleBinaryReader OpenRead(ObjectInfo info)
        {
            var swapEndian = SerializedFileHeader.HasEndian(_header.Version) ?
                _header.Endian : _metadata.SwapEndian;
            var reader = new BundleBinaryReader(
                new PartialStream(_reader.BaseStream, _header.DataOffset + info.ByteStart, info.ByteSize),
                swapEndian ? EndianType.BigEndian : EndianType.LittleEndian);
            reader.Add<ISerializedFile>(this);
            reader.Add(info);
            reader.Add(Version);
            reader.Add(Platform);
            reader.Add(Format);
            return reader;
        }

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


        public Stream OpenResource(ResourceSource source)
        {
            if (string.IsNullOrWhiteSpace(source.Source))
            {
                return new EmptyStream();
            }
            var stream = Container?.OpenResource(source.Source, this);
            if (stream is null)
            {
                return new EmptyStream();
            }
            return new PartialStream(stream, source.Offset, source.Size);
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

        public bool TryGet<T>(PPtr ptr, [NotNullWhen(true)] out T? instance)
        {
            if (TryGetResource(ptr.FileID, out var sourceFile))
            {
                var i = sourceFile.IndexOf(ptr.PathID);
                if (i >= 0)
                {
                    if (sourceFile[i] is T variable)
                    {
                        instance = variable;
                        return true;
                    }
                    instance = Container!.ConvertTo<T>(sourceFile, i);
                    return instance is not null;
                }
            }
            if (ptr.PathID != 0 && ptr.FileID >= 0)
            {
                Logger?.Warning($"Need: [{ptr.FileID}]{ptr.PathID}");
            }
            instance = default;
            return false;
        }

        private bool TryGetResource(int fileId, [NotNullWhen(true)] out ISerializedFile? result)
        {
            if (fileId == 0)
            {
                result = this;
                return true;
            }
            result = null;
            if (fileId > 0 && fileId - 1 < Dependencies.Count())
            {
                var assetsManager = Container;
                if (assetsManager is null)
                {
                    return false;
                }
                var index = assetsManager.IndexOf(GetDependency(fileId - 1));

                if (index >= 0)
                {
                    result = assetsManager[index];
                    return result is not null;
                }
            }
            return false;
        }

     
    }
}
