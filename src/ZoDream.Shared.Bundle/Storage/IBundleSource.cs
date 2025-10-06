using System.Threading;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleSource: IBundleStorage
    {
        /// <summary>
        /// 获取文件的数量，必须先调用 Analyze 方法
        /// </summary>
        public uint Count { get; }

        /// <summary>
        /// 重新计算文件的数量
        /// </summary>
        /// <returns></returns>
        public uint Analyze(CancellationToken token = default);

        /// <summary>
        /// 获取相对路径
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string GetRelativePath(string filePath);
    }
}
