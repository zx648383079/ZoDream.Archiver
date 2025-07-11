using System;
using ZoDream.Shared.Interfaces;

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
        public void AddEntry(IFilePath source, long entryId);
        /// <summary>
        /// 文件包含编号的资源
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="entryId"></param>
        /// <param name="entryName">区分大小写</param>
        /// <param name="entryType"></param>
        public void AddEntry(string fileName, long entryId, string entryName, int entryType);
        public void AddEntry(IFilePath source, long entryId, string entryName, int entryType);
        /// <summary>
        /// 文件包含名称的资源
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="entryName">区分大小写</param>
        public void AddEntry(string fileName, string entryName);
        public void AddEntry(IFilePath source, string entryName);
        public void AddEntry(IFilePath source);
        /// <summary>
        /// 文件依赖编号的资源
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="dependencyEntryId"></param>
        public void AddDependencyEntry(string fileName, long dependencyEntryId);
        public void AddDependencyEntry(IFilePath source, long dependencyEntryId);
        /// <summary>
        /// 文件中的编号资源依赖另一个编号资源
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="entryId"></param>
        /// <param name="dependencyEntryId"></param>
        public void AddDependencyEntry(string fileName, long entryId, long dependencyEntryId);
        public void AddDependencyEntry(IFilePath source, long entryId, long dependencyEntryId);
        public void AddDependencyEntry(string fileName, long entryId, string dependencyEntryName);
        public void AddDependencyEntry(IFilePath source, long entryId, string dependencyEntryName);
        /// <summary>
        /// 文件中的名称资源依赖另一个名称资源
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="entryName"></param>
        /// <param name="dependencyEntryName">区分大小写</param>
        public void AddDependencyEntry(string fileName, string entryName, string dependencyEntryName);
        public void AddDependencyEntry(IFilePath source, string entryName, string dependencyEntryName);
        /// <summary>
        /// 文件依赖名称的资源
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="dependencyEntryName">区分大小写</param>
        public void AddDependencyEntry(string fileName, string dependencyEntryName);
        public void AddDependencyEntry(IFilePath source, string dependencyEntryName);
        /// <summary>
        /// 文件依赖另一个文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="dependencyFileName"></param>
        public void AddDependency(string fileName, string dependencyFileName);
        public void AddDependency(IFilePath source, IFilePath dependencyPath);

        /// <summary>
        /// 执行部分写入
        /// </summary>
        public void Flush();

        /// <summary>
        /// 转成文件依赖字典
        /// </summary>
        /// <returns></returns>
        public IDependencyDictionary ToDictionary();
        /// <summary>
        /// 指定 string 比较的方式 转成文件依赖字典
        /// </summary>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        public IDependencyDictionary ToDictionary(StringComparison comparisonType);
    }
}
