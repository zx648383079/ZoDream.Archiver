using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace UnityEngine
{
    public class ResourceManager(IResourceEntry resource) : Object
    {
        public KeyValuePair<string, PPtr>[] Container;

        public bool TryGet<T>(PPtr ptr, [NotNullWhen(true)] out T? instance)
        {
            return TryGet(ptr, out instance);
        }
    }
}
