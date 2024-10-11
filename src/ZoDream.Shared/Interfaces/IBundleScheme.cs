using System.Collections.Generic;

namespace ZoDream.Shared.Interfaces
{
    /// <summary>
    /// 项目文件管理
    /// </summary>
    public interface IBundleScheme
    {
        public IBundleReader? Load(IEnumerable<string> fileItems, IArchiveOptions? options = null);

    }
}
