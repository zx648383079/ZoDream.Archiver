using System.Collections.Generic;
using System.IO;
using System.Threading;
using ZoDream.BundleExtractor.Platforms;
using ZoDream.BundleExtractor.SerializedFiles;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor
{
    public class BundleChunkReader : IBundleReader
    {
        private readonly IEnumerable<string> _fileItems;
        private readonly IPlatformScheme _platform;
        private readonly UnityBundleScheme _scheme;

        private readonly List<SerializedFileReader> _assetItems = [];
        private readonly List<Stream> _resourceItems = [];
        private readonly List<string> _importItems = [];
        private readonly HashSet<string> _resourceFileHash = [];
        private readonly HashSet<string> _importFileHash = [];
        private readonly HashSet<string> _assetFileHash = [];

        public BundleChunkReader(
            IEnumerable<string> fileItems, 
            IPlatformScheme platform)
            : this(fileItems, null, platform)
        {
        }

        public BundleChunkReader(IEnumerable<string> fileItems,
            UnityBundleScheme? scheme,
            IPlatformScheme platform)
        {
            _scheme = scheme ?? new();
            _fileItems = fileItems;
            _platform = platform;
        }

        public void ExtractTo(string folder, CancellationToken token = default)
        {
            foreach (var item in _fileItems)
            {
                _importItems.Add(item);
                _importFileHash.Add(Path.GetFileName(item));
            }
            for (int i = 0; i < _importItems.Count; i++)
            {
                LoadFile(_importItems[i]);
            }
        }

        private void LoadFile(string fullName)
        {
            using var fs = File.OpenRead(fullName);
            using var reader =  _scheme.Open(fs, fullName, Path.GetFileName(fullName), new ArchiveOptions()
            {
                LeaveStreamOpen = false
            });

        }

        public void Dispose()
        {
            foreach (var item in _assetItems)
            {
                item.Dispose();
            }
            foreach (var item in _resourceItems)
            {
                item.Dispose();
            }
            _assetItems.Clear();
            _resourceItems.Clear();
            _assetFileHash.Clear();
            _importFileHash.Clear();
            _resourceFileHash.Clear();
        }
    }
}
