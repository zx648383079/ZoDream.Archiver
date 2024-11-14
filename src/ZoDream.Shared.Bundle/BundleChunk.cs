using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ZoDream.Shared.Bundle
{
    public class BundleChunk : IBundleChunk
    {
        public BundleChunk(string fileName)
            : this(fileName, "*.*")
        {
            
        }

        public BundleChunk(string fileName, string globPattern)
        {
            if (!string.IsNullOrWhiteSpace(globPattern))
            {
                _globPattern = globPattern;
            }
            if (!File.Exists(fileName))
            {
                Root = fileName;
                return;
            }
            Root = Path.GetDirectoryName(fileName)!;
            _fileItems = [fileName];
        }

        public BundleChunk(string baseFolder, IEnumerable<string> items)
        {
            Root = baseFolder;
            _fileItems = items;
        }

        private readonly IEnumerable<string>? _fileItems;
        private readonly string _globPattern = "*.*";
        public string Root { get; private set; }


        public string Create(string sourcePath, string outputFolder)
        {
            if (sourcePath.StartsWith(Root))
            {
                return Path.Combine(outputFolder, Path.GetRelativePath(Root, sourcePath));
            }
            if (sourcePath.StartsWith(outputFolder))
            {
                return sourcePath;
            }
            return Path.Combine(outputFolder, Path.GetFileName(sourcePath)); ;
        }

        public IEnumerator<string> GetEnumerator()
        {
            if (_fileItems is not null)
            {
                foreach (var item in _fileItems)
                {
                    yield return item;
                }
                yield break;
            }
            foreach (var item in Directory.GetFiles(Root, _globPattern, SearchOption.AllDirectories))
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_fileItems is not null)
            {
                return _fileItems.GetEnumerator();
            }
            return Directory.GetFiles(Root, _globPattern, SearchOption.AllDirectories).GetEnumerator();
        }
    }
}
