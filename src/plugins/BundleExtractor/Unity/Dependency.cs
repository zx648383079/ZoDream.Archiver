using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity
{
    internal class UTY_DependencyFile
    {
        public string[] files;

        public UTY_DependencyMap[] assetbundles;
    }
    internal class UTY_DependencyMap
    {
        public int n;
        public int[] d;
    }
}
