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
        public bool EnabledModel { get; set; } = true;
        public string ModelFormat { get; set; } = "glb";
    }
}
