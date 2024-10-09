using SharpCompress;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using ZoDream.BundleExtractor.Platforms;
using ZoDream.BundleExtractor.SerializedFiles;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor
{
    public partial class BundleChunkReader : IBundleReader, IBundleContainer
    {


        
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

        private readonly IEnumerable<string> _fileItems;
        private readonly IPlatformScheme _platform;
        private readonly UnityBundleScheme _scheme;

        private readonly List<ISerializedFile> _assetItems = [];
        private readonly Dictionary<string, int> _assetIndexItems = [];
        private readonly List<Stream> _resourceItems = [];
        private readonly List<string> _importItems = [];
        private readonly HashSet<string> _resourceFileHash = [];
        private readonly HashSet<string> _importFileHash = [];
        private readonly HashSet<string> _assetFileHash = [];

        public ISerializedFile? this[int index] => _assetItems[index];

        public int IndexOf(string fileName)
        {
            if (_assetIndexItems.TryGetValue(fileName, out var index))
            {
                return index;
            }
            var i = _assetItems.FindIndex(x => x.FullPath.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            _assetIndexItems.Add(fileName, i);
            return i;
        }

        private void AddDependency(IEnumerable<string> fileItems)
        {
            foreach (var item in _fileItems)
            {
                AddDependency(Path.GetFileName(item), item);
            }
        }

        private void AddDependency(string name, string fullPath)
        {
            if (_importFileHash.Contains(name))
            {
                return;
            }
            if (!File.Exists(fullPath))
            {
                return;
            }
            _importItems.Add(fullPath);
            _importFileHash.Add(Path.GetFileName(name));
        }

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
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

            ReadAssets();
            ProcessAssets();

            foreach (var asset in _assetItems)
            {
                foreach (var obj in asset.Children)
                {
                    var exportPath = Path.Combine(folder, asset.FullPath + "_export");
                    ExportConvertFile(obj, exportPath, mode);
                }
            }
        }

        
        private void LoadFile(string fullName)
        {
            using var fs = File.OpenRead(fullName);
            LoadFile(fs, fullName);
        }

        private void LoadFile(Stream stream, string fullName)
        {
            using var reader = _scheme.Open(stream, fullName, Path.GetFileName(fullName), new ArchiveOptions()
            {
                LeaveStreamOpen = false
            });
            if (reader is null)
            {
                _resourceItems.Add(stream);
                return;
            }
            if (reader is SerializedFileReader s)
            {
                s.Container = this;
                _assetItems.Add(s);
                s.Dependencies.ForEach(i => AddDependency(i, Path.Combine(Path.GetDirectoryName(fullName), i)));
                return;
            }
            var entries = reader.ReadEntry().ToArray();
            foreach (var item in entries)
            {
                using var ms = new MemoryStream();
                reader.ExtractTo(item, ms);
                ms.Position = 0;
                LoadFile(stream, item.Name);
            }
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
