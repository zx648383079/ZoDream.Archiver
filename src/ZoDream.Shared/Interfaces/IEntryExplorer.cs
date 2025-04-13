using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Interfaces
{
    public interface ISourceEntry : IReadOnlyEntry
    {
        public bool IsDirectory { get; }

        public string FullPath { get; }
    }

    public interface IEntryStream
    {
        
    }

    public interface IEntryService
    {
        public void Add<T>(T instance);
        public void Add(string key, object? instance);
        public void AddIf<T>();
        public T Get<T>();
        public object Get(Type type);
        public T Get<T>(string key);

        public bool TryGet<T>([NotNullWhen(true)] out T? instance);
        public bool TryGet<T>(string key, [NotNullWhen(true)] out T? instance);

        public Task<T?> AskAsync<T>();
        /// <summary>
        /// 判断存档点是否存在
        /// </summary>
        /// <param name="hashCode"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public bool TryLoadPoint(int hashCode, out uint record);
        public bool CheckPoint(int hashCode);
        /// <summary>
        /// 保存存档点
        /// </summary>
        /// <param name="hashCode"></param>
        /// <param name="record"></param>
        public void SavePoint(int hashCode, uint record);
    }

    public interface IEntryExplorer: IDisposable
    {

        public bool CanGoBack { get; }

        public Task<IEntryStream> OpenAsync(ISourceEntry entry);

        public void SaveAs(ISourceEntry entry, Stream output);
        public void SaveAs(ISourceEntry entry, string folder,
            ArchiveExtractMode mode,
            CancellationToken token = default);
    }
}
