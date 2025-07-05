using System;
using System.IO;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleRequest
    {
        public long RequestId { get; }
        public IBundleExecutor? Executor { get; }

        public IBundleToken Token { get; }
    }

    public interface IFileRequest : IBundleRequest
    {
        public string Name { get; }

        public Stream OpenRead();
    }

    public interface INetRequest : IBundleRequest
    {
        public Uri Source { get; }
        /// <summary>
        /// 保存的文件夹
        /// </summary>
        public string Output {  get; }

        public string? SuggestedName { get; }
    }

    public interface INetFileRequest : IFileRequest
    {
        public Uri Source { get; }
    }
}