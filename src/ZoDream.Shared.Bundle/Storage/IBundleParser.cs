using System.IO;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleParser
    {
        public IBundleBinaryReader Parse(Stream input, IFilePath sourcePath);
    }
}
