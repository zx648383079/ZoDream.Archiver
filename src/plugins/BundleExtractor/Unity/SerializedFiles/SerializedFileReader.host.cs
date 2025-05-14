using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Document;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Logging;
using ZoDream.Shared.Models;
using Object = UnityEngine.Object;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.SerializedFiles
{
    internal partial class SerializedFileReader : ISerializedFile
    {

        private readonly ImmutableDictionary<long, int> _objectIdMap;
        private readonly HashSet<int> _excludeItems = [];
        /// <summary>
        /// 跟 _metadata.Object 一一对应
        /// </summary>
        private readonly Object?[] _children;
        public IBundleContainer? Container { get; set; }

        public ILogger? Logger => Container?.Logger;
        public string FullPath { get; private set; }
        public FormatVersion Format => _header.Version;
        public Version Version => _metadata.Version;

        public BuildTarget Platform => _metadata.TargetPlatform;

        public int Count => _metadata.Object.Length;

        /// <summary>
        /// 依赖的完整路径
        /// </summary>
        public IList<string> Dependencies { get; private set; } = [];

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
        /// <param name="fileId">资源的编号</param>
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
        /// <param name="index">序号</param>
        /// <returns></returns>
        public bool IsExclude(int index)
        {
            if (index < 0)
            {
                return false;
            }
            return _excludeItems.Contains(index);
        }

        public string GetDependency(int index)
        {
            return Dependencies[index];
        }

        public int AddDependency(string dependency)
        {
            Dependencies.Add(dependency);
            return Dependencies.Count - 1;
        }

        public int IndexOf(string dependency)
        {
            for (int i = 0; i < Dependencies.Count; i++)
            {
                if (Dependencies[i].Equals(dependency, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
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

    }
}
