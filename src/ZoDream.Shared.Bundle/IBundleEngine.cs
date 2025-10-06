namespace ZoDream.Shared.Bundle
{
    public interface IBundleEngine: IBundleLoader
    {
        public IBundleSplitter CreateSplitter(IBundleOptions options);

        public IBundleHandler CreateHandler(IBundleChunk fileItems, IBundleOptions options);

        public IBundleSource Unpack(IBundleSource fileItems, IBundleOptions options);
        
        /// <summary>
        /// 创造一个依赖关系绑定器
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IDependencyBuilder GetBuilder(IBundleOptions options);
        
    }
}
