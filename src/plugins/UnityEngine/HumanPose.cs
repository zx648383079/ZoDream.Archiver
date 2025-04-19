using System.Numerics;

namespace UnityEngine
{
    public class HumanPose
    {
        public Transform RootX { get; set; }
        public Vector3 LookAtPosition { get; set; }
        public Vector4 LookAtWeight { get; set; }
        public HumanGoal[] GoalArray { get; set; }
        public HandPose LeftHandPose { get; set; }
        public HandPose RightHandPose { get; set; }
        public float[] DoFArray { get; set; }
        public Vector3[] TDoFArray { get; set; }
    }
}
