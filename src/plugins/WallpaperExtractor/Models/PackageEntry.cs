using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Models;

namespace ZoDream.WallpaperExtractor.Models
{
    public class PackageEntry : ReadOnlyEntry
    {
        public PackageEntry(string name, long length) : base(name, length)
        {
            
        }

        public PackageEntry(string name, int offset, long length)
            :this(name, length)
        {
            Offset = offset;
        }

        public long Offset { get; set; }


    }
}
