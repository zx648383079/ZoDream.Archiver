using System;

namespace ZoDream.Shared.Bundle
{
    public interface IDependencyBuilder : IDisposable
    {
        /// <summary>
        /// 文件包含编号的资源
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="entryId"></param>
        public void AddEntry(string fileName, long entryId);
        /// <summary>
        /// 文件包含编号的资源
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="entryId"></param>
        /// <param name="entryName"></param>
        /// <param name="entryType"></param>
        public void AddEntry(string fileName, long entryId, string entryName, int entryType);
        /// <summary>
        /// 文件包含名称的资源
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="entryName"></param>
        public void AddEntry(string fileName, string entryName);
        /// <summary>
        /// 文件依赖编号的资源
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="dependencyEntryId"></param>
        public void AddDependencyEntry(string fileName, long dependencyEntryId);
        /// <summary>
        /// 文件中的编号资源依赖另一个编号资源
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="entryId"></param>
        /// <param name="dependencyEntryId"></param>
        public void AddDependencyEntry(string fileName, long entryId, long dependencyEntryId);
        public void AddDependencyEntry(string fileName, long entryId, string dependencyEntryName);
        /// <summary>
        /// 文件中的名称资源依赖另一个名称资源
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="entryName"></param>
        /// <param name="dependencyEntryName"></param>
        public void AddDependencyEntry(string fileName, string entryName, string dependencyEntryName);
        /// <summary>
        /// 文件依赖名称的资源
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="dependencyEntryName"></param>
        public void AddDependencyEntry(string fileName, string dependencyEntryName);
        /// <summary>
        /// 文件依赖另一个文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="dependencyFileName"></param>
        public void AddDependency(string fileName, string dependencyFileName);

        /// <summary>
        /// 执行部分写入
        /// </summary>
        public void Flush();

        public IDependencyDictionary ToDictionary();
    }
}
