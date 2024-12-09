using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleArchiveScheme
    {
        public IArchiveReader? Open(IBundleBinaryReader reader, string filePath, string fileName, IArchiveOptions? options = null);

    }
}
