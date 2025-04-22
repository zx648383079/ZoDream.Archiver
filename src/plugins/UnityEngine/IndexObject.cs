using System.Collections.Generic;

namespace UnityEngine
{
    public class Index
    {
        public IPPtr<Object> Object;
        public ulong Size;
    }

    public sealed class IndexObject : Object
    {
        public KeyValuePair<string, Index>[] AssetMap;
    }
}
