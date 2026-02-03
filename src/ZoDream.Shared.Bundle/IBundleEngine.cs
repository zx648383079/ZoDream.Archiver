using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;

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
        /// <summary>
        /// 识别文件内容
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<ICommandArguments?> RecognizeAsync(IStorageFileEntry filePath, CancellationToken token = default);
    }
}
