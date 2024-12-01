using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class AvatarConstant
    {
        public Skeleton m_AvatarSkeleton;
        public SkeletonPose m_AvatarSkeletonPose;
        public SkeletonPose m_DefaultPose;
        public uint[] m_SkeletonNameIDArray;
        public Human m_Human;
        public int[] m_HumanSkeletonIndexArray;
        public int[] m_HumanSkeletonReverseIndexArray;
        public int m_RootMotionBoneIndex;
        public XForm<Vector3> m_RootMotionBoneX;
        public Skeleton m_RootMotionSkeleton;
        public SkeletonPose m_RootMotionSkeletonPose;
        public int[] m_RootMotionSkeletonIndexArray;

        public AvatarConstant(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();
            m_AvatarSkeleton = new Skeleton(reader);
            m_AvatarSkeletonPose = new SkeletonPose(reader);

            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                m_DefaultPose = new SkeletonPose(reader);

                m_SkeletonNameIDArray = reader.ReadArray(r => r.ReadUInt32());
            }

            m_Human = new Human(reader);

            m_HumanSkeletonIndexArray = reader.ReadArray(r => r.ReadInt32());

            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                m_HumanSkeletonReverseIndexArray = reader.ReadArray(r => r.ReadInt32());
            }

            m_RootMotionBoneIndex = reader.ReadInt32();
            m_RootMotionBoneX = reader.ReadXForm();

            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                m_RootMotionSkeleton = new Skeleton(reader);
                m_RootMotionSkeletonPose = new SkeletonPose(reader);

                m_RootMotionSkeletonIndexArray = reader.ReadArray(r => r.ReadInt32());
            }
        }
    }

}
