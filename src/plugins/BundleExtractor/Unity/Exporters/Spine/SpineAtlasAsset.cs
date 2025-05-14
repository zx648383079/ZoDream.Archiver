using UnityEngine;

namespace ZoDream.BundleExtractor.Unity.Spine
{
    internal class SpineAtlasAsset : MonoBehaviour
    {
        public IPPtr<TextAsset> AtlasFile { get; set; }
        public IPPtr<Material>[] Materials { get; set; }
    }
}
