using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ZoDream.BundleExtractor.Platforms;
using ZoDream.BundleExtractor.Unity;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor
{
    internal partial class UnityBundleChunkReader : IBundleReader, IBundleContainer
    {

        public UnityBundleChunkReader(
            IBundleChunk fileItems,
            IEntryService service,
            IBundleOptions options)
        {
            _fileItems = fileItems;
            _options = options;
            _service = service;
            _scheme = service.Get<UnityBundleScheme>() ?? new();
        }

        private readonly IBundleChunk _fileItems;
        private readonly IBundleOptions _options;
        private readonly IEntryService _service;
        private readonly UnityBundleScheme _scheme;

        private readonly List<ISerializedFile> _assetItems = [];
        private readonly ConcurrentDictionary<string, int> _assetIndexItems = [];
        private readonly ConcurrentDictionary<string, Stream> _resourceItems = [];
        private readonly List<string> _importItems = [];
        private readonly HashSet<string> _resourceFileHash = [];
        private readonly HashSet<string> _importFileHash = [];
        private readonly HashSet<string> _assetFileHash = [];
        private readonly HashSet<long> _excludeItems = [];

        public ISerializedFile? this[int index] => _assetItems[index];
        public ILogger Logger => _service.Get<ILogger>();

        public IBundleExtractOptions Options => (IBundleExtractOptions)_options;
        /// <summary>
        /// 添加一个不需要导出
        /// </summary>
        /// <param name="fileId"></param>
        public void TryAddExclude(long fileId)
        {
            _excludeItems.Add(fileId);
        }
        /// <summary>
        /// 判断一个对象不需要导出
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public bool IsExclude(long fileId)
        {
            return _excludeItems.Contains(fileId);
        }

        public int IndexOf(string fileName)
        {
            if (_assetIndexItems.TryGetValue(fileName, out var index))
            {
                return index;
            }
            var i = _assetItems.FindIndex(x => x.FullPath.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            _assetIndexItems.TryAdd(fileName, i);
            return i;
        }
        private void AddDependency(IEnumerable<string> fileItems, string entryPath)
        {
            foreach (var item in _fileItems)
            {
                AddDependency(Path.GetFileName(item), FileNameHelper.CombineBrother(entryPath, item));
            }
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
            Logger.Info("Load file...");
            for (int i = 0; i < _importItems.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                LoadFile(_importItems[i], token);
            }
            var onlyDependencyTask = _options is IBundleExtractOptions o && o.OnlyDependencyTask;
            Logger.Info(onlyDependencyTask ? "Build dependencies ... " : "Read assets...");
            ReadAssets(onlyDependencyTask, token);
            if (onlyDependencyTask)
            {
                return;
            }
            Logger.Info("Process assets...");
            ProcessAssets(token);
            Logger.Info("Export assets...");
            ExportAssets(folder, mode, token);
        }

        private void LoadFile(string fullName, CancellationToken token)
        {
            var reader = _service.Get<IBundleStorage>().OpenRead(fullName);
            if (reader is null || reader.Length == 0)
            {
                reader?.Dispose();
                return;
            }
            try
            {
                LoadFile(reader, fullName, token);
            }
            catch (Exception e)
            {
                // Logger.Debug(fullName);
                Logger.Error(e.Message);
            }
        }

        private void LoadFile(Stream stream, string fullName, CancellationToken token)
        {
            var reader = _service.Get<IBundleStorage>().OpenRead(stream, fullName);
            if (reader is null || reader.Length == 0)
            {
                reader?.Dispose();
                return;
            }
            try
            {
                LoadFile(reader, fullName, token);
            }
            catch (Exception e)
            {
                // Logger.Debug(fullName);
                Logger.Error(e.Message);
            }
        }
        private void LoadFile(IBundleBinaryReader stream, string fullName, CancellationToken token)
        {
            var temporary = _service.Get<ITemporaryStorage>();
            temporary.Add(stream.BaseStream);
            stream.Add(_service.Get<IBundleCodec>());
            var name = FileNameHelper.GetFileName(fullName);
            using var reader = _scheme.Open(stream, fullName, name, new ArchiveOptions()
            {
                LeaveStreamOpen = true
            });
            if (token.IsCancellationRequested)
            {
                stream.Dispose();
                return;
            }
            if (reader is null)
            {
                _resourceItems.TryAdd(name, stream.BaseStream);
                // stream.Dispose();
                return;
            }
            if (reader is SerializedFileReader s)
            {
                s.Container = this;
                _assetItems.Add(s);
                AddDependency(s.Dependencies, fullName);
                temporary.Add(s);
                return;
            }
            var entries = reader.ReadEntry().ToArray();
            foreach (var item in entries)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                var ms = temporary.Create();
                reader.ExtractTo(item, ms);
                ms.Position = 0;
                LoadFile(ms, FileNameHelper.Combine(fullName, item.Name), token);
            }
            stream.BaseStream.Dispose();
            stream.Dispose();
        }

        public Stream OpenResource(string fileName, ISerializedFile source)
        {
            fileName = Path.GetFileName(fileName);
            if (_resourceItems.TryGetValue(fileName, out var stream))
            {
                return stream;
            }
            var assetsFileDirectory = Path.GetDirectoryName(source.FullPath);
            var resourceFilePath = Path.Combine(assetsFileDirectory, fileName);
            if (!File.Exists(resourceFilePath))
            {
                var findFiles = Directory.GetFiles(assetsFileDirectory, fileName, SearchOption.AllDirectories);
                if (findFiles.Length > 0)
                {
                    resourceFilePath = findFiles[0];
                }
            }
            if (File.Exists(resourceFilePath))
            {
                if (_resourceItems.TryGetValue(fileName, out stream))
                {
                    return stream;
                }
                stream = File.OpenRead(resourceFilePath);
                _resourceItems.TryAdd(fileName, stream);
                _service.Get<ITemporaryStorage>().Add(stream);
                return stream;
            }
            return new EmptyStream();
        }


        public void Dispose()
        {
            foreach (var item in _assetItems)
            {
                item.Dispose();
            }
            foreach (var item in _resourceItems)
            {
                item.Value.Dispose();
            }
            _assetItems.Clear();
            _resourceItems.Clear();
            _assetFileHash.Clear();
            _importFileHash.Clear();
            _resourceFileHash.Clear();
        }
    }
}
