using System.Numerics;
using ZoDream.Shared.Numerics;

namespace UnityEngine
{
    public class ClipMuscleConstant
    {
        public HumanPose DeltaPose { get; set; }
        public Transform<Vector3> StartX { get; set; }
        public Transform<Vector3> StopX { get; set; }
        public Transform<Vector3> LeftFootStartX { get; set; }
        public Transform<Vector3> RightFootStartX { get; set; }
        public Transform<Vector3> MotionStartX { get; set; }
        public Transform<Vector3> MotionStopX { get; set; }
        public Vector3 AverageSpeed { get; set; }
        public Clip Clip { get; set; }
        public float StartTime { get; set; }
        public float StopTime { get; set; }
        public float OrientationOffsetY { get; set; }
        public float Level { get; set; }
        public float CycleOffset { get; set; }
        public float AverageAngularSpeed { get; set; }
        public int[] IndexArray { get; set; }
        public ValueDelta[] ValueArrayDelta { get; set; }
        public float[] ValueArrayReferencePose { get; set; }
        public bool Mirror { get; set; }
        public bool LoopTime { get; set; }
        public bool LoopBlend { get; set; }
        public bool LoopBlendOrientation { get; set; }
        public bool LoopBlendPositionY { get; set; }
        public bool LoopBlendPositionXZ { get; set; }
        public bool StartAtOrigin { get; set; }
        public bool KeepOriginalOrientation { get; set; }
        public bool KeepOriginalPositionY { get; set; }
        public bool KeepOriginalPositionXZ { get; set; }
        public bool HeightFromFeet { get; set; }

    }
}
