using System.Collections.Generic;
using System.Numerics;

namespace UnityEngine
{
    public class ClipMuscleConstant
    {
        public HumanPose DeltaPose { get; set; }
        public Transform StartX { get; set; }
        public Transform StopX { get; set; }
        public Transform LeftFootStartX { get; set; }
        public Transform RightFootStartX { get; set; }
        public Transform MotionStartX { get; set; }
        public Transform MotionStopX { get; set; }
        public Vector3 AverageSpeed { get; set; }
        public Clip Clip { get; set; }
        public float StartTime { get; set; }
        public float StopTime { get; set; }
        public float OrientationOffsetY { get; set; }
        public float Level { get; set; }
        public float CycleOffset { get; set; }
        public float AverageAngularSpeed { get; set; }
        public int[] IndexArray { get; set; }
        public List<ValueDelta> ValueArrayDelta { get; set; }
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
