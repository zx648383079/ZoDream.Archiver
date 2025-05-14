using UnityEngine;

namespace ZoDream.BundleExtractor.Unity.Live2d
{
    internal sealed class CubismExpressionData : MonoBehaviour
    {
        public string Type { get; set; }
        public float FadeInTime { get; set; }
        public float FadeOutTime { get; set; }
        public SerializableExpressionParameter[] Parameters { get; set; }

        public struct SerializableExpressionParameter
        {
            public string Id { get; set; }
            public float Value { get; set; }
            public BlendType Blend { get; set; }
        }
        public enum BlendType : byte
        {
            Add,
            Multiply,
            Overwrite,
        }
    }
}
