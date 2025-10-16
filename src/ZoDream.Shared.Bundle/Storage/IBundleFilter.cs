using System.Collections.Generic;
using System.Collections.ObjectModel;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle
{
    /// <summary>
    /// 文件过滤程序
    /// </summary>
    public interface IBundleFilter
    {
        /// <summary>
        /// 是否匹配到
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>true 为排除</returns>
        public bool IsMatch(IFilePath filePath);
    }

    public class BundleMultipleFilter : Collection<IBundleFilter>, IBundleFilter
    {
        public BundleMultipleFilter()
        {
            
        }
        public BundleMultipleFilter(IList<IBundleFilter> items): base(items)
        {
            
        }
        public bool IsMatch(IFilePath filePath)
        {
            foreach (var item in Items)
            {
                if (item.IsMatch(filePath))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
