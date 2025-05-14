using UnityEngine;

namespace ZoDream.BundleExtractor.Unity.Live2d
{
    internal sealed class CubismFadeMotionList : MonoBehaviour
    {
        public int[] MotionInstanceIds { get; set; }
        public IPPtr<CubismFadeMotionData>[] CubismFadeMotionObjects { get; set; }
    }
}
