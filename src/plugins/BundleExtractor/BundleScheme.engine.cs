using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme
    {
        private IBundleEngine[] _engineItems = [];

        public string[] EngineNames => GetNames(_engineItems);

    }
}
