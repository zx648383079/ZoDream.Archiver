using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleExtractOptions : IArchiveExtractOptions
    {
        /// <summary>
        /// 最大批处理数量
        /// </summary>
        public int MaxBatchCount { get; }
        public bool EnabledImage { get; }
        public bool EnabledVideo { get; }
        public bool EnabledAudio { get; }
        public bool EnabledModel { get; }
        public string ModelFormat { get; }
    }
}
