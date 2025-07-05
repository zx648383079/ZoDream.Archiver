namespace ZoDream.Shared.Bundle
{
    public enum BundleStatus : byte
    {
        None,
        Waiting,
        Sending,
        Receiving,
        Working,
        Paused,
        Completed,      // 已完成
        Failed,         // 失败
        Cancelled,       // 已取消
    }
}
