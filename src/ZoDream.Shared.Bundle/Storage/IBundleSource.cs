using System.Collections.Generic;
using System.Threading;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleSource: IEnumerable<string>
    {
        /// <summary>
        /// 当前执行的位置
        /// </summary>
        public uint Index { get; set; }
        /// <summary>
        /// 获取文件的数量，必须先调用 Analyze 方法
        /// </summary>
        public uint Count { get; }

        /// <summary>
        /// 重新计算文件的数量
        /// </summary>
        /// <returns></returns>
        public uint Analyze(CancellationToken token = default);
        public uint Analyze(IBundleFilter filter, CancellationToken token = default);

        public IEnumerable<string> GetFiles(params string[] searchPatternItems);
        public IEnumerable<string> GetDirectories(params string[] searchPatternItems);
        /// <summary>
        /// 获取所有的，不区分文件还是文件夹
        /// </summary>
        /// <param name="searchPatternItems"></param>
        /// <returns></returns>
        public IEnumerable<string> Glob(params string[] searchPatternItems);

        /// <summary>
        /// 拆分组
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IBundleChunk> EnumerateChunk();
        /// <summary>
        /// 根据文件数量拆分组
        /// </summary>
        /// <param name="maxFileCount"></param>
        /// <returns></returns>
        public IEnumerable<IBundleChunk> EnumerateChunk(int maxFileCount);

        /// <summary>
        /// 根据依赖拆分组
        /// </summary>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        public IEnumerable<IBundleChunk> EnumerateChunk(IDependencyDictionary dependencies);
    }

    public interface IBundleSourceFilter
    {
        /// <summary>
        /// 在执行的过程中需要排除一些重复执行的文件
        /// </summary>
        /// <param name="filePath"></param>
        public void Exclude(string filePath);
        /// <summary>
        /// 判断文件是否是需要排除的已执行过的
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool IsExclude(string filePath);
    }
}
