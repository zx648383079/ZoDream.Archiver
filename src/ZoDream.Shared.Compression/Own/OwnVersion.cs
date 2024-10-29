namespace ZoDream.Shared.Compression.Own
{
    public enum OwnVersion: byte
    {
        Unknown,
        V1,
        /// <summary>
        /// 增加分块, 名称16/块, 流64/块，前一加密块 ^ 当前加密块
        /// </summary>
        V2,
    }
}
