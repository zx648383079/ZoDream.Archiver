using System.Numerics;
using ZoDream.Shared.Numerics;

namespace UnityEngine
{
    public class AvatarConstant
    {
        public Skeleton AvatarSkeleton;
        public SkeletonPose AvatarSkeletonPose;
        public SkeletonPose DefaultPose;
        public uint[] SkeletonNameIDArray;
        public Human Human;
        public int[] HumanSkeletonIndexArray;
        public int[] HumanSkeletonReverseIndexArray;
        public int RootMotionBoneIndex;
        public Transform<Vector3> RootMotionBoneX;
        public Skeleton RootMotionSkeleton;
        public SkeletonPose RootMotionSkeletonPose;
        public int[] RootMotionSkeletonIndexArray;

    }

}
