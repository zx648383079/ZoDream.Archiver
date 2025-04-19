namespace UnityEngine
{
    public class Human
    {
        public Transform RootX;
        public Skeleton Skeleton;
        public SkeletonPose SkeletonPose;
        public Hand LeftHand;
        public Hand RightHand;
        public Handle[] Handles;
        public Collider[] ColliderArray;
        public int[] HumanBoneIndex;
        public float[] HumanBoneMass;
        public int[] ColliderIndex;
        public float Scale;
        public float ArmTwist;
        public float ForeArmTwist;
        public float UpperLegTwist;
        public float LegTwist;
        public float ArmStretch;
        public float LegStretch;
        public float FeetSpacing;
        public bool HasLeftHand;
        public bool HasRightHand;
        public bool HasTDoF;
    }
}
