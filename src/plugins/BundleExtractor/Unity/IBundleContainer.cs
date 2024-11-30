using System.IO;

namespace ZoDream.BundleExtractor.Unity
{
    internal interface IBundleContainer
    {
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
    }
}
