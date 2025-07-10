using System.Collections.Generic;
using System.IO;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle
{
    public class BundleEntry : ReadOnlyEntry, IBundleEntry
    {
        public long Id {  get; private set; }

        public int Type { get; private set; }

        public long Offset { get; private set; }

        public BundleEntry(string name)
            : base(name)
        {
            
        }

        public BundleEntry(long id, string name, int type)
            : base(name)
        {
            Id = id;
            Type = type;
        }

        public BundleEntry(long id, string name, int type, long offset, long length)
            : base(name, length)
        {
            Id = id;
            Type = type;
            Offset = offset;
        }
    }

    public class BundleEntrySource(string fileName) : List<IBundleEntry>, IBundleEntrySource
    {
        public string Name { get; private set; } = Path.GetFileName(fileName);

        public string FullPath => fileName;

        public long[] LinkedItems { get; set; } = [];
        public string[] LinkedPartItems { get; set; } = [];
    }
}
