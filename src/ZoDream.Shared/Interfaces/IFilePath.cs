using System;

namespace ZoDream.Shared.Interfaces
{

    public interface IFileName : IEquatable<IFileName>, IEquatable<IFilePath>, IEquatable<IEntryPath>, IEquatable<string>
    {
        public string Name { get; }

        public bool Equals(IFileName? other, StringComparison comparisonType);
    }

    public interface IFilePath : IFileName, IEquatable<IFilePath>, IEquatable<string>
    {
        public string FullPath { get; }

        public bool Equals(IFilePath? other, StringComparison comparisonType);

        /// <summary>
        /// 从文件打开 entry
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IFilePath Combine(string name);
        /// <summary>
        /// 变更为同级的其他 entry
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IFilePath Adjacent(string name);
    }

    /// <summary>
    /// 只知道 EntryName 用来进行查找
    /// </summary>
    public interface IEntryName : IFileName, IEquatable<IEntryName>, IEquatable<IEntryPath>, IEquatable<string>
    {
        public string EntryPath { get; }

        public bool Equals(IEntryName? other, StringComparison comparisonType);
    }

    public interface IEntryPath : IFilePath, IEntryName, IEquatable<IEntryPath>, IEquatable<IEntryName>
    {
        public string FilePath { get; }

        public bool Equals(IEntryPath? other, StringComparison comparisonType);
    }
  
}
