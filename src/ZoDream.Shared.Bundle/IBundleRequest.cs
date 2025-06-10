using System;
using System.IO;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleRequest
    {

        public IBundleExecutor? Executor { get; }
    }

    public interface IFileRequest : IBundleRequest
    {
        public string Name { get; }

        public Stream OpenRead();
    }

    public interface INetRequest : IBundleRequest
    {
        public Uri Source { get; }
    }

    public interface INetFileRequest : IFileRequest
    {
        public Uri Source { get; }
    }
}