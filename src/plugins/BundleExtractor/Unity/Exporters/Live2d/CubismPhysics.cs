using System.Numerics;
using UnityEngine;

namespace ZoDream.BundleExtractor.Unity.Live2d
{
    internal struct CubismPhysicsNormalizationTuplet
    {
        public float Maximum { get; set; }
        public float Minimum { get; set; }
        public float Default { get; set; }
    }

    internal struct CubismPhysicsNormalization
    {
        public CubismPhysicsNormalizationTuplet Position { get; set; }
        public CubismPhysicsNormalizationTuplet Angle { get; set; }
    }

    internal struct CubismPhysicsParticle
    {
        public Vector2 InitialPosition { get; set; }
        public float Mobility { get; set; }
        public float Delay { get; set; }
        public float Acceleration { get; set; }
        public float Radius { get; set; }
    }

    internal struct CubismPhysicsOutput
    {
        public string DestinationId { get; set; }
        public int ParticleIndex { get; set; }
        public Vector2 TranslationScale { get; set; }
        public float AngleScale { get; set; }
        public float Weight { get; set; }
        public CubismPhysicsSourceComponent SourceComponent { get; set; }
        public bool IsInverted { get; set; }
    }

    internal enum CubismPhysicsSourceComponent : byte
    {
        X,
        Y,
        Angle,
    }

    internal struct CubismPhysicsInput
    {
        public string SourceId { get; set; }
        public Vector2 ScaleOfTranslation { get; set; }
        public float AngleScale { get; set; }
        public float Weight { get; set; }
        public CubismPhysicsSourceComponent SourceComponent { get; set; }
        public bool IsInverted { get; set; }
    }

    internal struct CubismPhysicsSubRig
    {
        public CubismPhysicsInput[] Input { get; set; }
        public CubismPhysicsOutput[] Output { get; set; }
        public CubismPhysicsParticle[] Particles { get; set; }
        public CubismPhysicsNormalization Normalization { get; set; }
    }

    internal class CubismPhysicsRig
    {
        public CubismPhysicsSubRig[] SubRigs { get; set; }
        public Vector2 Gravity { get; set; } = new Vector2(0, -1);
        public Vector2 Wind { get; set; }
        public float Fps { get; set; }
    }

    internal sealed class CubismPhysics : MonoBehaviour
    {
        public CubismPhysicsRig Rig { get; set; }
    }
}
