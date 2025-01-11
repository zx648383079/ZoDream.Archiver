using ElectronExtractor;
using System.IO;
using System.Threading.Tasks;
using ZoDream.ChmExtractor;
using ZoDream.EpubExtractor;
using ZoDream.Shared.Compression;
using ZoDream.Shared.Interfaces;
using ZoDream.WallpaperExtractor;

namespace ZoDream.Archiver.ViewModels
{
    public class PluginViewModel
    {

        private readonly IArchiveScheme[] PluginItems = [
            new ChmScheme(),
            new PackageScheme(),
            new EpubScheme(),
            new ElectronScheme(),
            new CompressScheme(),
        ];
        
        public IArchiveReader? TryGetReader(Stream stream,
            string filePath,
            IArchiveOptions? options, 
            out IArchiveScheme? scheme)
        {
            var fileName = Path.GetFileName(filePath);
            foreach (var item in PluginItems)
            {
                stream.Seek(0, SeekOrigin.Begin);
                var reader = item.Open(stream, filePath, fileName, options);
                if (reader is not null)
                {
                    scheme = item;
                    return reader;
                }
            }
            scheme = null;
            return null;
        }

        public async Task<IArchiveReader?> GetReaderAsync(Stream stream,
            string filePath,
            IArchiveOptions? options)
        {
            var fileName = Path.GetFileName(filePath);
            foreach (var item in PluginItems)
            {
                stream.Seek(0, SeekOrigin.Begin);
                var reader = await item.OpenAsync(stream, filePath, fileName, options);
                if (reader is not null)
                {
                    return reader;
                }
            }
            return null;
        }
    }
}
