using System.Collections.Generic;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleSource: IEnumerable<string>
    {
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
}
