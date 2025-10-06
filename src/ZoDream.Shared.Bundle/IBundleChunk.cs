using System.Collections.Generic;
using System.IO;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleChunk
    {
        /// <summary>
        /// 所有文件的数量
        /// </summary>
        public int Count { get; }


        public IEnumerable<IFilePath> Items { get; }
        public IEnumerable<IFilePath> Dependencies { get; }


        public Stream OpenRead(IFilePath filePath);
        public Stream OpenWrite(IFilePath filePath);

        /// <summary>
        /// 生成输出文件夹的新路径
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="fileName"></param>
        /// <param name="outputFolder"></param>
        /// <returns></returns>
        public string Create(IFilePath sourcePath, string outputFolder);
        public string Create(IFilePath sourcePath, string fileName, string outputFolder);
    }
}
