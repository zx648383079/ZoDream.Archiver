using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleExtractOptions : IArchiveExtractOptions
    {
        /// <summary>
        /// 最大批处理数量
        /// </summary>
        public int MaxBatchCount { get; }
        /// <summary>
        /// 当路径不存在时则为先建依赖
        /// </summary>
        public string DependencySource { get; }

        public bool EnabledImage { get; }
        public bool EnabledVideo { get; }
        public bool EnabledAudio { get; }
        public bool EnabledShader { get; }
        public bool EnabledLua { get; }
        /// <summary>
        /// 包括 live2d
        /// </summary>
        public bool EnabledSpine { get; }
        public bool EnabledModel { get; }
        public string ModelFormat { get; }
        /// <summary>
        /// 当前是否只执行建立依赖关系任务
        /// </summary>
        public bool OnlyDependencyTask { get; }
    }
}
