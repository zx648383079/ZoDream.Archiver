using System.Collections.Generic;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle
{
    /// <summary>
    /// 项目文件管理
    /// </summary>
    public interface IBundleScheme
    {
        /// <summary>
        /// 提取
        /// </summary>
        /// <param name="fileItems"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public IBundleReader? Load(IEnumerable<string> fileItems, IArchiveOptions? options = null);
        /// <summary>
        /// 获取项目信息
        /// </summary>
        /// <param name="fileItems"></param>
        /// <returns></returns>
        public IBundleOptions? TryLoad(IEnumerable<string> fileItems);

    }
}
