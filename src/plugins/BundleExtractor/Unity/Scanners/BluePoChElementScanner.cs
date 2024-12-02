using System.IO;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class BluePoChElementScanner(string package) : IBundleElementScanner, IBundleStorage
    {
        public bool IsReverse1999 => package.Contains("re1999");

        public Stream Open(string path)
        {
            return File.OpenRead(path);
        }

        public bool TryRead(IBundleBinaryReader reader, object instance)
        {
            if (instance is IElementLoader l)
            {
                l.Read(reader);
                return true;
            }
            return false;
        }
    }
}
