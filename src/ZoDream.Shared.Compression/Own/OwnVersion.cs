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
        /// <summary>
        /// 分块，默认 1024 组成 64 * 8 矩阵 * 2 矩阵竖着读写， 不足 n * 2 * 2, 最小 n * 2, 
        /// </summary>
        V3,
    }
}
