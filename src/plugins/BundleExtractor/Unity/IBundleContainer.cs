using System.IO;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Logging;

namespace ZoDream.BundleExtractor.Unity
{
    internal interface IBundleContainer
    {
        public ILogger? Logger { get; }

        public IAssemblyReader Assembly { get; }
        public IBundleExtractOptions Options { get; }

        /// <summary>
        /// 判断文件在 Asset 中的位置
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public int IndexOf(string fileName);

        public ISerializedFile? this[int index] { get; }
        /// <summary>
        /// 获取资源文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public Stream OpenResource(string fileName, ISerializedFile source);
        /// <summary>
        /// 转换为
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asset"></param>
        /// <param name="entryId">资源的序号</param>
        /// <returns></returns>
        public T? ConvertTo<T>(ISerializedFile asset, int entryId);
    }
}
