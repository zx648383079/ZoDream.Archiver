namespace ZoDream.Shared.Bundle
{
    public interface IBundleProducer: IBundleLoader
    {
        /// <summary>
        /// 获取特殊的文件系统
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IBundleStorage GetStorage(IBundleOptions options);

        /// <summary>
        /// 获取文件解析器
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IBundleElementScanner GetScanner(IBundleOptions options);
    }
}
