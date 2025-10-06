namespace ZoDream.Shared.Bundle
{
    public interface IBundleProducer: IBundleLoader
    {
        /// <summary>
        /// 获取特殊的文件系统
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IBundleParser CreateParser(IBundleOptions options);

        /// <summary>
        /// 获取文件解析器
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IBundleSerializer CreateSerializer(IBundleOptions options);
    }
}
