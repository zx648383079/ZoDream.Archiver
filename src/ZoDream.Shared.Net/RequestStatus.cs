namespace ZoDream.Shared.Net
{
    public enum RequestStatus : byte
    {
        None,
        Waiting,
        Paused,
        Sending,
        Receiving,
        Finished,
        Canceled,
        /// <summary>
        /// 出错了
        /// </summary>
        Occurred,
    }
}
