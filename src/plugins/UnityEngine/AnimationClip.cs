namespace UnityEngine
{
    public sealed class AnimationClip : Motion
    {
        public AnimationType AnimationType;
        public bool Legacy;
        public bool Compressed;
        public bool UseHighQualityCurve;
        public QuaternionCurve[] RotationCurves;
        public CompressedAnimationCurve[] CompressedRotationCurves;
        public Vector3Curve[] EulerCurves;
        public Vector3Curve[] PositionCurves;
        public Vector3Curve[] ScaleCurves;
        public FloatCurve[] FloatCurves;
        public PPtrCurve[] PPtrCurves;
        public float SampleRate;
        public int WrapMode;
        public Bounds Bounds;
        public uint MuscleClipSize;
        public ClipMuscleConstant MuscleClip;
        public AnimationClipBindingConstant ClipBindingConstant;
        public AnimationEvent[] Events;
        public ResourceSource StreamData;
    }
}
