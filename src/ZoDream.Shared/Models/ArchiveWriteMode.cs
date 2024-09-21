namespace ZoDream.Shared.Models
{
    /// <summary>
    /// 解压文件，遇到同名文件
    /// </summary>
    public enum ArchiveExtractMode
    {
        None,
        Overwrite,
        Skip,
        Rename,
    }
}
