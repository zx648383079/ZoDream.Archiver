using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using Object = UnityEngine.Object;

namespace ZoDream.BundleExtractor.Unity
{
    internal class ObjectPPtr<T>: IPPtr<T>
        where T : Object
    {
        public ObjectPPtr(ISerializedFile resource)
        {
            _resource = resource;
        }
        public ObjectPPtr(ISerializedFile resource, PPtr ptr)
        {
            FileID = ptr.FileID;
            PathID = ptr.PathID;
            _resource = GetResource(resource, FileID);
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ISerializedFile? _resource;
        /// <summary>
        /// 之后不在更新 _resource
        /// </summary>
        public int FileID { get; private set; }

        public long PathID { get; private set; }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IResourceEntry? Resource => _resource;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public int Index => IsNotNull ? _resource.IndexOf(PathID) : -1;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsValid => Index >= 0 && _resource[Index] is T;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsNotNull => PathID != 0 && FileID >= 0 && _resource is not null;
#if DEBUG
        public Object? Target => Index >= 0 ? _resource[Index] : null;
#endif

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsExclude 
        {
            get => IsNotNull && _resource?.IsExclude(Index) == true;
            set {
                if (value && IsNotNull)
                {
                    _resource?.AddExclude(PathID);
                }
            }
        }

        public IPPtr<K> Create<K>(IPPtr ptr) where K : Object
        {
            if (ptr == this)
            {
                return new ObjectPPtr<K>(_resource)
                {
                    FileID = ptr.FileID,
                    PathID = ptr.PathID
                };
            }
            var source = ptr is PPtr o ? o : new PPtr()
            {
                FileID = FileID,
                PathID = PathID,
            };
            return new ObjectPPtr<K>(_resource, source);
        }

        public void Set(ObjectInfo entry, ISerializedFile instanceSource)
        {
            var name = instanceSource.FullPath;
            var source = _resource is null ? instanceSource : _resource;
            if (source.FullPath.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                FileID = 0;
            }
            else
            {
                FileID = source.IndexOf(name);
                if (FileID == -1)
                {
                    FileID = source.AddDependency(instanceSource.FullPath);
                }
                FileID += 1;
            }
            var host = source.Container!;
            var index = host.IndexOf(name);
            if (index < 0)
            {
                source.Logger?.Warning($"Need: {name}[{FileID}]{PathID}");
                _resource = null;
            } else
            {
                _resource = host[index];
            }
            PathID = entry.FileID;
        }

        private static ISerializedFile? GetResource(ISerializedFile resource, int fileId)
        {
            if (fileId == 0)
            {
                return resource;
            }
            if (fileId > 0 && fileId - 1 < resource.Dependencies.Count)
            {
                var assetsManager = resource.Container;
                if (assetsManager is null)
                {
                    return null;
                }
                var name = resource.GetDependency(fileId - 1);
                var index = assetsManager.IndexOf(name);
                if (index < 0)
                {
                    resource.Logger?.Warning($"Need<{resource.FullPath}>: {name}[{fileId}]");
                    return null;
                }
                else
                {
                    return assetsManager[index];
                }
            }
            return null;
        }

        public bool TryGet([NotNullWhen(true)] out T? instance)
        {
            return TryGet<T>(out instance);
        }
        public bool TryGet<K>([NotNullWhen(true)] out K? instance)
        {
            if (_resource is null)
            {
                instance = default;
                return false;
            }
            var i = Index;
            if (i >= 0)
            {
                if (_resource[i] is K variable)
                {
                    instance = variable;
                    return true;
                }
                instance = _resource.Container!.ConvertTo<K>(_resource, i);
                return instance is not null;
            }
            instance = default;
            return false;
        }

        public override bool Equals(object? obj)
        {
            if (obj is IPPtr ptr)
            {
                return ptr.FileID == FileID && ptr.PathID == PathID;
            }
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return $"[PPtr<{typeof(T).Name}>]{FileID}:{PathID}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FileID, PathID);
        }
    }
}
