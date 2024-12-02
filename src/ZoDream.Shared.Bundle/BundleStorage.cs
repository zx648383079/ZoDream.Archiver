using System.IO;

namespace ZoDream.Shared.Bundle
{
    public class BundleStorage : IBundleStorage
    {
        public Stream Open(string path)
        {
            return File.OpenRead(path);
        }
    }
}
