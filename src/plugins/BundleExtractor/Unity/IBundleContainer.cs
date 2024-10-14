using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity
{
    public interface IBundleContainer
    {

        public int IndexOf(string fileName);

        public ISerializedFile? this[int index] { get; }
    }
}
