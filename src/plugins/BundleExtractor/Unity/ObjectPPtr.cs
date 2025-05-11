using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using Object = UnityEngine.Object;

namespace ZoDream.BundleExtractor.Unity
{
    internal class ObjectPPtr<T>(ISerializedFile resource, PPtr ptr): IPPtr<T>
        where T : Object
    {
        private int _index = -2; //-2 - Prepare, -1 - Missing
        public PPtr Value => ptr;

        public int FileID => ptr.FileID;

        public long PathID => ptr.PathID;
        public bool IsNull => ptr.PathID == 0 || ptr.FileID < 0;

        public void Set(ObjectInfo entry, ISerializedFile instanceSource)
        {
            var name = instanceSource.FullPath;
            if (string.Equals(resource.FullPath, name, StringComparison.OrdinalIgnoreCase))
            {
                ptr.FileID = 0;
            }
            else
            {
                ptr.FileID = resource.IndexOf(name);
                if (ptr.FileID == -1)
                {
                    ptr.FileID = resource.AddDependency(instanceSource.FullPath);
                }
                ptr.FileID += 1;
            }

            _index = resource.Container!.IndexOf(name);

            ptr.PathID = entry.FileID;
        }

        private bool TryGetResource([NotNullWhen(true)] out ISerializedFile? result)
        {
            if (ptr.FileID == 0)
            {
                result = resource;
                return true;
            }
            result = null;
            if (ptr.FileID > 0 && ptr.FileID - 1 < resource.Dependencies.Count())
            {
                var assetsManager = resource.Container;
                if (assetsManager is null)
                {
                    return false;
                }
                if (_index == -2)
                {
                    var name = resource.GetDependency(ptr.FileID - 1);
                    _index = assetsManager.IndexOf(name);
                }

                if (_index >= 0)
                {
                    result = assetsManager[_index];
                    return result is not null;
                }
            }
            return false;
        }

        public bool TryGet([NotNullWhen(true)] out T? instance)
        {
            return TryGet<T>(out instance);
        }
        public bool TryGet<K>([NotNullWhen(true)] out K? instance)
        {
            if (TryGetResource(out var sourceFile))
            {
                var i = sourceFile.IndexOf(ptr.PathID);
                if (i >= 0)
                {
                    if (sourceFile[i] is K variable)
                    {
                        instance = variable;
                        return true;
                    }
                    instance = sourceFile.Container!.ConvertTo<K>(sourceFile, i);
                    return instance is not null;
                }
            }
            if (ptr.PathID != 0 && ptr.FileID >= 0)
            {
                resource.Logger?.Warning($"Need: [{ptr.FileID}]{ptr.PathID}");
            }
            instance = default;
            return false;
        }
    }
}
