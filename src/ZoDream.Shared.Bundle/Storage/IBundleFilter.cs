using System;

namespace ZoDream.Shared.Bundle
{
    /// <summary>
    /// 文件过滤程序
    /// </summary>
    public interface IBundleFilter
    {
        /// <summary>
        /// 事先排除
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool IsExclude(string filePath);
        public bool IsExclude(string filePath, BundleExcludeFlag flag);

        /// <summary>
        /// 在执行的过程中需要排除一些重复执行的文件
        /// </summary>
        /// <param name="filePath"></param>
        public void Exclude(string filePath, BundleExcludeFlag flag);
        /// <summary>
        /// 判断文件是否是需要排除的已执行过的
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool IsExportable(string filePath);
        /// <summary>
        /// 重置
        /// </summary>
        public void Reset();
    }
    [Flags]
    public enum BundleExcludeFlag: byte
    {
        None = 0b0,
        Import = 0b1,
        Export = 0b10,
    }


}
