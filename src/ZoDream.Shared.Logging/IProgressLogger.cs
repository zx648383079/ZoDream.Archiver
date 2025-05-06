namespace ZoDream.Shared.Logging
{
    public interface IProgressLogger
    {
        /// <summary>
        /// 是否是主线程
        /// </summary>
        public bool IsMaster { get; }

        public string Title { get; set; }

        public long Max { get; set; }

        public long Value { get; set; }
    }
}