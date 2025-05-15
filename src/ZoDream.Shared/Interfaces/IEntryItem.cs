namespace ZoDream.Shared.Interfaces
{
    public interface IEntryItem
    {
        public bool IsDirectory { get; }
        /// <summary>
        /// 文件拓展名，包含 .
        /// </summary>
        public string Extension { get; }
    }
}
