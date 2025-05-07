namespace ZoDream.Shared.Models
{
    /// <summary>
    /// 解压文件，遇到同名文件
    /// </summary>
    public enum ArchiveExtractMode : byte
    {
        None,
        Overwrite,
        Skip,
        Rename,
    }
}
