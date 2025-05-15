namespace ZoDream.Shared.Net
{
    public enum RequestStatus : byte
    {
        None,
        Waiting,
        Sending,
        Receiving,
        Finished,
        Paused,
        Canceled,
        /// <summary>
        /// 出错了
        /// </summary>
        Occurred,
    }
}
