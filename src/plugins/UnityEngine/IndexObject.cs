using System.Collections.Generic;

namespace UnityEngine
{
    public class Index
    {
        public PPtr Object;
        public ulong Size;
    }

    public sealed class IndexObject : Object
    {
        public KeyValuePair<string, Index>[] AssetMap;
    }
}
