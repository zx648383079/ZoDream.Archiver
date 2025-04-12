using System.Collections.Generic;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleEngine: IBundleLoader
    {
        /// <summary>
        /// 排除一个文件
        /// </summary>
        /// <param name="options"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool IsExclude(IBundleOptions options, string fileName);

        public IEnumerable<IBundleChunk> EnumerateChunk(IBundleSource fileItems, IBundleOptions options);

        public IBundleReader OpenRead(IBundleChunk fileItems, IBundleOptions options);
        
        /// <summary>
        /// 创造一个依赖关系绑定器
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IDependencyBuilder GetBuilder(IBundleOptions options);
    }
}
