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
    internal partial class UnityBundleChunkReader : IBundleHandler, IBundleContainer
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
        private readonly ConcurrentDictionary<IFileName, int> _assetIndexItems = [];
        private readonly ConcurrentDictionary<string, KeyValuePair<IFilePath, Stream>> _resourceItems = [];
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
                    instance.Load(FileNameHelper.CombineIf(Path.GetDirectoryName(_options.Entrance), "DummyDll"));
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
        /// <param name="fileName">文件名/entryName</param>
        /// <returns></returns>
        public int IndexOf(IFileName fileName)
        {
            if (_assetIndexItems.TryGetValue(fileName, out var index))
            {
                return index;
            }
            var i = _assetItems.FindIndex(x => x.FullPath.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            _assetIndexItems.TryAdd(fileName, i);
            return i;
        }
        /// <summary>
        /// 注册依赖
        /// </summary>
        /// <param name="fileItems">依赖项</param>
        /// <param name="entryPath">来源文件</param>
        private void AddDependency(IEnumerable<IFileName> fileItems, IFilePath entryPath)
        {
            foreach (var item in fileItems)
            {
                AddDependency(entryPath, item);
            }
        }
        /// <summary>
        /// 注册依赖
        /// </summary>
        /// <param name="entryPath">来源文件</param>
        /// <param name="dependency">依赖项</param>
        private void AddDependency(IFilePath entryPath, IFileName dependency)
        {
            if (dependency is IEntryPath e)
            {
                _dependency?.AddDependency(entryPath, e);
                EnqueueImportQueue(e.FilePath);
                return;
            }
            if (dependency is IFilePath f)
            {
                _dependency?.AddDependency(entryPath, f);
                EnqueueImportQueue(f.FullPath);
                return;
            }
            if (dependency is IEntryName n)
            {
                _dependency?.AddDependencyEntry(entryPath, n.Name);
                return;
            }
            if (_importFileHash.Contains(dependency.Name))
            {
                return;
            }
            var dependencyPath = FindFile(dependency.Name, entryPath);
            if (string.IsNullOrEmpty(dependencyPath))
            {
                return;
            }
            _dependency?.AddDependency(entryPath, new FilePath(dependencyPath));
            EnqueueImportQueue(dependencyPath);
        }

        private void EnqueueImportQueue(IFilePath fullPath)
        {
            EnqueueImportQueue(fullPath.Name, fullPath.FullPath);
        }
        private void EnqueueImportQueue(string fullPath)
        {
            EnqueueImportQueue(Path.GetFileName(fullPath), fullPath);
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
            _importItems.Add(fullPath);
            _importFileHash.Add(Path.GetFileName(name));
        }

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            _extractMode = mode;
            _extractFolder = folder;
            var entryItems = new HashSet<string>(); 
            foreach (var item in _fileItems.Items)
            {
                EnqueueImportQueue(item);
                entryItems.Add(item.FullPath);
            }
            foreach (var item in _fileItems.Dependencies)
            {
                EnqueueImportQueue(item);
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
            ExportAssets(entryItems, folder, mode, token);
            ExportResource(entryItems, folder, mode, token);
        }

        private void LoadFile(string fullName, CancellationToken token)
        {
            var reader = _fileItems.OpenRead(FilePath.Parse(fullName));
            if (reader is null || reader.Length == 0)
            {
                reader?.Dispose();
                return;
            }
            try
            {
                LoadFile(reader, new FilePath(fullName), token);
            }
            catch (Exception e)
            {
                // Logger.Debug(fullName);
                Logger?.Error(e.Message);
            }
        }

        private void LoadFile(Stream stream, IFilePath fullName, CancellationToken token)
        {
            var reader = _service.Get<IBundleParser>().Parse(stream, fullName);
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
            IFilePath fullName, 
            CancellationToken token)
        {
            var temporary = _service.Get<ITemporaryStorage>();
            temporary.Add(stream.BaseStream);
            stream.Add(_service.Get<IBundleCodec>());
            using var reader = _scheme.Open(stream, fullName, new ArchiveOptions()
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
                _dependency?.AddEntry(fullName);
                _resourceItems.TryAdd(fullName.Name, new KeyValuePair<IFilePath, Stream>(fullName, stream.BaseStream));
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
                var entryPath = fullName.Combine(item.Name);
                _dependency?.AddEntry(entryPath);
                if (reader is IRawEntryReader r && r.TryExtractTo(item, _extractFolder, _extractMode))
                {
                    continue;
                }
                var ms = temporary.Create();
                reader.ExtractTo(item, ms);
                ms.Position = 0;

                LoadFile(ms, entryPath, token);
            }
            stream.BaseStream.Dispose();
            stream.Dispose();
        }

        public Stream OpenResource(string fileName, ISerializedFile source)
        {
            fileName = Path.GetFileName(fileName);
            if (_resourceItems.TryGetValue(fileName, out var target))
            {
                return target.Value;
            }
            var resourceFilePath = FindFile(fileName, source.FullPath);
            if (File.Exists(resourceFilePath))
            {
                if (_resourceItems.TryGetValue(fileName, out target))
                {
                    return target.Value;
                }
                var stream = File.OpenRead(resourceFilePath);
                _resourceItems.TryAdd(fileName, new KeyValuePair<IFilePath, Stream>(
                    new FilePath(resourceFilePath),
                    stream));
                _service.Get<ITemporaryStorage>().Add(stream);
                return stream;
            }
            Logger?.Warning($"Need<{source.FullPath}>: {fileName}");
            return new EmptyStream();
        }

        /// <summary>
        /// 查找文件, 从 source 所在文件下找，从入口文件下找
        /// </summary>
        /// <param name="name"></param>
        /// <param name="source">触发文件</param>
        /// <returns></returns>
        private string FindFile(string name, IFilePath source)
        {
            var folder = Path.GetDirectoryName(FilePath.GetFilePath(source));
            if (!string.IsNullOrEmpty(folder))
            {
                var filePath = Path.Combine(folder, name);
                if (File.Exists(filePath))
                {
                    return filePath;
                }
                foreach (var item in BundleStorage.GetFiles(folder, name))
                {
                    return item;
                }
            }
            folder = _options.Entrance;
            if (!string.IsNullOrEmpty(folder))
            {
                foreach (var item in BundleStorage.GetFiles(folder, name))
                {
                    return item;
                }
            }
            return string.Empty;
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
                item.Value.Value.Dispose();
            }
            _assetItems.Clear();
            _resourceItems.Clear();
            _assetFileHash.Clear();
            _importFileHash.Clear();
            _resourceFileHash.Clear();
        }
    }
}
