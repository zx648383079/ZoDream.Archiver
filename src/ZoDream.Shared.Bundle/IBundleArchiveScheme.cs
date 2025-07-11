using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleArchiveScheme
    {
        public IArchiveReader? Open(IBundleBinaryReader reader, IFilePath sourcePath, IArchiveOptions? options = null);

    }
}
