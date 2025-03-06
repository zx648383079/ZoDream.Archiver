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

        public string Create(string sourcePath, string outputFolder);
    }
}
