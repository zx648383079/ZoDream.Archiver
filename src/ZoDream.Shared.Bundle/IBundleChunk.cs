using System.Collections.Generic;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleChunk: IEnumerable<string>
    {
        /// <summary>
        /// 所有文件的数量
        /// </summary>
        public int Count { get; }
        /// <summary>
        /// 当前的进度
        /// </summary>
        public int Index { get; }
        /// <summary>
        /// 生成输出文件夹的新路径
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="outputFolder"></param>
        /// <returns></returns>
        public string Create(string sourcePath, string outputFolder);
    }
}
