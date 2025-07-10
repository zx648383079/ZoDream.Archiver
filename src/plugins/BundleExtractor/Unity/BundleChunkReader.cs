using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using ZoDream.BundleExtractor.Producers;
using ZoDream.BundleExtractor.Unity;
using ZoDream.BundleExtractor.Unity.Document;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Logging;
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
            var onlyDependencyTask = _options is IBundleExtractOptions o && o.OnlyDependencyTask;
            _dependency = onlyDependencyTask ? _service.Get<IDependencyBuilder>() : null;
        }

        private readonly IBundleChunk _fileItems;
        private readonly IBundleOptions _options;
        private readonly IEntryService _service;
        private readonly UnityBundleScheme _scheme;
        private readonly IDependencyBuilder? _dependency;
        private ArchiveExtractMode _extractMode;
        private string _extractFolder = string.Empty;
        private readonly List<ISerializedFile> _assetItems = [];
        private readonly ConcurrentDictionary<string, int> _assetIndexItems = [];
        private readonly ConcurrentDictionary<string, Stream> _resourceItems = [];
        private readonly List<string> _importItems = [];
        private readonly HashSet<string> _resourceFileHash = [];
        private readonly HashSet<string> _importFileHash = [];
        private readonly HashSet<string> _assetFileHash = [];

        public IBundleSharedBag Shared { get; private set; } = new BundleSharedBag();
        public IAssemblyReader Assembly 
        {
            get {
                if (_service.TryGet<IAssemblyReader>(out var instance))
                {
                    return instance;
                }
                instance = new AssemblyReader();
                if (!string.IsNullOrWhiteSpace(_options.Entrance))
                {
                    instance.Load(Path.Combine(Path.GetDirectoryName(_options.Entrance), "DummyDll"));
                }
                _service.Add(instance);
                return instance;
            }
        }
        public ISerializedFile? this[int index] => _assetItems[index];
        public ILogger? Logger => _service.Get<ILogger>();

        public IBundleExtractOptions Options => (IBundleExtractOptions)_options;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">{触发依赖路径}#{依赖内容}</param>
        /// <returns></returns>
        public int IndexOf(string fileName)
        {
            // # 前面的触发路径不一定准确
            if (_assetIndexItems.TryGetValue(fileName, out var index))
            {
                return index;
            }
            var i = _assetItems.FindIndex(x => x.FullPath.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            if (i == -1)
            {
                BundleStorage.Separate(fileName, out var entryName);
                if (!string.IsNullOrEmpty(entryName))
                {
                    i = _assetItems.FindIndex(x => BundleStorage.IsEntryName(x.FullPath, entryName, StringComparison.OrdinalIgnoreCase));
                }
            }
            _assetIndexItems.TryAdd(fileName, i);
            return i;
        }
        /// <summary>
        /// 注册依赖
        /// </summary>
        /// <param name="fileItems">依赖项</param>
        /// <param name="entryPath">来源文件</param>
        private void AddDependency(IEnumerable<string> fileItems, string entryPath)
        {
            foreach (var item in fileItems)
            {
                var target = BundleStorage.Separate(item, out var targetEntry);
                if (!string.IsNullOrEmpty(targetEntry))
                {
                    _dependency?.AddDependencyEntry(entryPath, targetEntry);
                } else
                {
                    _dependency?.AddDependency(entryPath, target);
                }
                EnqueueImportQueue(Path.GetFileName(target), target);
            }
        }
        /// <summary>
        /// 添加文件到需要导入的队列
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fullPath"></param>
        private void EnqueueImportQueue(string name, string fullPath)
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
            _extractMode = mode;
            _extractFolder = folder;
            foreach (var item in _fileItems)
            {
                _importItems.Add(item);
                _importFileHash.Add(Path.GetFileName(item));
            }
            var progress = Logger?.CreateSubProgress("Load file...", _importItems.Count);
            for (int i = 0; i < _importItems.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                LoadFile(_importItems[i], token);
                if (progress is not null)
                {
                    progress.Max = _importItems.Count;
                    progress.Value = i;
                }
            }
            if (_dependency is not null)
            {
                return;
            }
            ReadAssets(token);
            ProcessAssets(token);
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
                Logger?.Error(e.Message);
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
                Logger?.Error(e.Message);
            }
        }
        private void LoadFile(IBundleBinaryReader stream, 
            string fullName, 
            CancellationToken token)
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
                _dependency?.AddEntry(fullName, name);
                _resourceItems.TryAdd(name, stream.BaseStream);
                // stream.Dispose();
                return;
            }
            if (reader is SerializedFileReader s)
            {
                s.Container = this;
                if (s.Version.Type == VersionType.TuanJie && 
                    _service.TryGet<bool>(UnknownProducer.CheckKey, out var check) && check)
                {
                    // 切换引擎
                    BundleReader.AddProducer(_service, new TuanJieProducer());
                    _service.Add(UnknownProducer.CheckKey, false);
                }
                _assetItems.Add(s);
                // 绑定文件依赖
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
                if (reader is IRawEntryReader r && r.TryExtractTo(item, _extractFolder, _extractMode))
                {
                    continue;
                }
                var ms = temporary.Create();
                reader.ExtractTo(item, ms);
                ms.Position = 0;
                LoadFile(ms, BundleStorage.Combine(fullName, item.Name), token);
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
            Logger?.Warning($"Need: {fileName}");
            return new EmptyStream();
        }


        public void Dispose()
        {
            Shared.Dispose();
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
