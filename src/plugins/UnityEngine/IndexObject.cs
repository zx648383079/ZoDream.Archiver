using System.Collections.Generic;

namespace UnityEngine
{
    public class Index
    {
        public PPtr<Object> Object;
        public ulong Size;
    }

    internal sealed class IndexObject : Object
    {
        public int Count;
        public List<KeyValuePair<string, Index>> AssetMap;
    }
}
