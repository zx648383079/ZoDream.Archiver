using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.Shared.Bundle
{
    public class BundleChunk(IBundleSource source, IFilePath[] chunkItems, IFilePath[] dependencies) : IBundleChunk
    {

        public BundleChunk(IBundleSource source, string[] chunkItems, string[] dependencies)
            : this(source, chunkItems.Select(i => (IFilePath)new FilePath(i)).ToArray(), 
                  dependencies.Select(i => (IFilePath)new FilePath(i)).ToArray())
        {
            
        }

        /// <summary>
        /// 所有文件的数量
        /// </summary>
        public int Count => chunkItems.Length;

        public IEnumerable<IFilePath> Items => chunkItems;
        public IEnumerable<IFilePath> Dependencies => dependencies;

        public Stream OpenRead(IFilePath filePath)
        {
            return source.OpenRead(filePath);
        }
        public Stream OpenWrite(IFilePath filePath)
        {
            return source.OpenWrite(filePath);
        }

        public string Create(IFilePath sourcePath, string outputFolder)
        {
            return Create(sourcePath, string.Empty, outputFolder);
        }

        public string Create(IFilePath sourcePath, string fileName, string outputFolder)
        {
            var fullPath = FilePath.GetFilePath(sourcePath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = sourcePath.Name;
            }
            var sourceFolder = Path.GetDirectoryName(fullPath);
            if (!LocationStorage.IsFullPath(fullPath))
            {
                return Path.Combine(outputFolder, sourceFolder, LocationStorage.CreateSafeFileName(fileName));
            }
            if (sourceFolder?.StartsWith(outputFolder) == true)
            {
                return Path.Combine(sourceFolder, LocationStorage.CreateSafeFileName(fileName));
            }
            return Path.Combine(outputFolder, source.GetRelativePath(sourceFolder), LocationStorage.CreateSafeFileName(fileName));
        }

      
    }
}
