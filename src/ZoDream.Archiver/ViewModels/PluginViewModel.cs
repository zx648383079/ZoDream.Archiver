using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Compression;
using ZoDream.Shared.Interfaces;
using ZoDream.WallpaperExtractor;

namespace ZoDream.Archiver.ViewModels
{
    public class PluginViewModel
    {

        private IArchiveScheme[] PluginItems = [
            new PackageScheme(),
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
    }
}
