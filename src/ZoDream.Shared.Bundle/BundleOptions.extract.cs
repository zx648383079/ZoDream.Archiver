using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle
{
    public partial class BundleOptions: IBundleExtractOptions
    {
        

        public string OutputFolder { get; set; } = string.Empty;

        public ArchiveExtractMode FileMode { get; set; } = ArchiveExtractMode.Overwrite;

        public int MaxBatchCount { get; set; } = 100;

        

        public bool EnabledImage { get; set; } = true;
        public bool EnabledVideo { get; set; } = true;
        public bool EnabledAudio { get; set; } = true;

        public bool EnabledShader { get; set; } = true;
        public bool EnabledLua { get; set; } = true;
        public bool EnabledJson { get; set; } = true;
        /// <summary>
        /// 包括 live2d
        /// </summary>
        public bool EnabledSpine { get; set; } = true;
        public bool EnabledModel { get; set; } = true;
        public bool EnabledResource { get; set; } = true;
        /// <summary>
        /// 单独的 Mesh obj 格式 
        /// </summary>
        public bool EnabledMesh { get; set; } = true;
        public string ModelFormat { get; set; } = "glb";

        #region 依赖关系

        public string DependencySource { get; set; } = string.Empty;
        public bool OnlyDependencyTask { get; set; }
        public string TypeTree { get; set; } = string.Empty;
        #endregion

    }
}
