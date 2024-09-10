using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Shared.Interfaces
{
    public interface IBundleScheme
    {
        public IBundleReader? Load(string folder, IArchiveOptions? options = null);

    }
}
