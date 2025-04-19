using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public class AssetBundle : Object
    {
        public PPtr<Object>[] PreloadTable;
        public KeyValuePair<string, AssetInfo>[] Container;
    }

    public class AssetInfo
    {
        public int PreloadIndex;
        public int PreloadSize;
        public PPtr<Object> Asset;
    }
}
