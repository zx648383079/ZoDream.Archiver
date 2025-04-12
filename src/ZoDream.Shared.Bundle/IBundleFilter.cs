namespace ZoDream.Shared.Bundle
{
    /// <summary>
    /// 文件过滤程序
    /// </summary>
    public interface IBundleFilter
    {
        public bool IsExclude(string fileName);
    }
}
