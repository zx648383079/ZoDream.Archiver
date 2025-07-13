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
        /// 真正导出的文件数量，一些通过 IsExportable 排除的文件不算
        /// </summary>
        public int EffectiveCount { get; }

        public IBundleSourceFilter? Filter { get; set; }
        /// <summary>
        /// 生成输出文件夹的新路径
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="outputFolder"></param>
        /// <returns></returns>
        public string Create(string sourcePath, string outputFolder);

        /// <summary>
        /// 路径是否能够进行导出，可以用来排除依赖的导出
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <returns></returns>

        public bool IsExportable(string sourcePath);
        /// <summary>
        /// 继承原本的入口，包装文件
        /// </summary>
        /// <param name="fileItems"></param>
        /// <returns></returns>
        public IBundleChunk Repack(IEnumerable<string> fileItems);
    }
}
