using UnityEngine;

namespace ZoDream.BundleExtractor.Unity.Live2d
{
    internal sealed class CubismFadeMotionData : MonoBehaviour
    {
        public string MotionName { get; set; }
        public float FadeInTime { get; set; }
        public float FadeOutTime { get; set; }
        public string[] ParameterIds { get; set; } = [];
        public AnimationCurve<float>[] ParameterCurves { get; set; }
        public float[] ParameterFadeInTimes { get; set; }
        public float[] ParameterFadeOutTimes { get; set; }
        public float MotionLength { get; set; }
    }
}
