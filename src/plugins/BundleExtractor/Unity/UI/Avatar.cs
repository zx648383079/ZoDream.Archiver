using System.Collections.Generic;
using System.Numerics;

namespace ZoDream.BundleExtractor.Unity.UI
{
    public class Node
    {
        public int m_ParentId;
        public int m_AxesId;

        public Node(UIReader reader)
        {
            m_ParentId = reader.ReadInt32();
            m_AxesId = reader.ReadInt32();
        }
    }

    public class Limit
    {
        public object m_Min;
        public object m_Max;

        public Limit(UIReader reader)
        {
            var version = reader.Version;
            if (version.GreaterThanOrEquals(5, 4))//5.4 and up
            {
                m_Min = reader.ReadVector3();
                m_Max = reader.ReadVector3();
            }
            else
            {
                m_Min = reader.ReadVector4();
                m_Max = reader.ReadVector4();
            }
        }
    }

    public class Axes
    {
        public Vector4 m_PreQ;
        public Vector4 m_PostQ;
        public object m_Sgn;
        public Limit m_Limit;
        public float m_Length;
        public uint m_Type;

        public Axes(UIReader reader)
        {
            var version = reader.Version;
            m_PreQ = reader.ReadVector4();
            m_PostQ = reader.ReadVector4();
            if (version.GreaterThanOrEquals(5, 4)) //5.4 and up
            {
                m_Sgn = reader.ReadVector3();
            }
            else
            {
                m_Sgn = reader.ReadVector4();
            }
            m_Limit = new Limit(reader);
            m_Length = reader.ReadSingle();
            m_Type = reader.ReadUInt32();
        }
    }

    public class Skeleton
    {
        public List<Node> m_Node;
        public uint[] m_ID;
        public List<Axes> m_AxesArray;

        public Skeleton(UIReader reader)
        {
            int numNodes = reader.ReadInt32();
            m_Node = new List<Node>();
            for (int i = 0; i < numNodes; i++)
            {
                m_Node.Add(new Node(reader));
            }

            m_ID = reader.ReadArray(r => r.ReadUInt32());

            int numAxes = reader.ReadInt32();
            m_AxesArray = new List<Axes>();
            for (int i = 0; i < numAxes; i++)
            {
                m_AxesArray.Add(new Axes(reader));
            }
        }
    }

    public class SkeletonPose
    {
        public XForm<Vector3>[] m_X;

        public SkeletonPose(UIReader reader)
        {
            m_X = reader.ReadXFormArray();
        }
    }

    public class Hand
    {
        public int[] m_HandBoneIndex;

        public Hand(UIReader reader)
        {
            m_HandBoneIndex = reader.ReadArray(r => r.ReadInt32());
        }
    }

    public class Handle
    {
        public XForm<Vector3> m_X;
        public uint m_ParentHumanIndex;
        public uint m_ID;

        public Handle(UIReader reader)
        {
            m_X = reader.ReadXForm();
            m_ParentHumanIndex = reader.ReadUInt32();
            m_ID = reader.ReadUInt32();
        }
    }

    public class Collider
    {
        public XForm<Vector3> m_X;
        public uint m_Type;
        public uint m_XMotionType;
        public uint m_YMotionType;
        public uint m_ZMotionType;
        public float m_MinLimitX;
        public float m_MaxLimitX;
        public float m_MaxLimitY;
        public float m_MaxLimitZ;

        public Collider(UIReader reader)
        {
            m_X = reader.ReadXForm();
            m_Type = reader.ReadUInt32();
            m_XMotionType = reader.ReadUInt32();
            m_YMotionType = reader.ReadUInt32();
            m_ZMotionType = reader.ReadUInt32();
            m_MinLimitX = reader.ReadSingle();
            m_MaxLimitX = reader.ReadSingle();
            m_MaxLimitY = reader.ReadSingle();
            m_MaxLimitZ = reader.ReadSingle();
        }
    }

    public class Human
    {
        public XForm<Vector3> m_RootX;
        public Skeleton m_Skeleton;
        public SkeletonPose m_SkeletonPose;
        public Hand m_LeftHand;
        public Hand m_RightHand;
        public List<Handle> m_Handles;
        public List<Collider> m_ColliderArray;
        public int[] m_HumanBoneIndex;
        public float[] m_HumanBoneMass;
        public int[] m_ColliderIndex;
        public float m_Scale;
        public float m_ArmTwist;
        public float m_ForeArmTwist;
        public float m_UpperLegTwist;
        public float m_LegTwist;
        public float m_ArmStretch;
        public float m_LegStretch;
        public float m_FeetSpacing;
        public bool m_HasLeftHand;
        public bool m_HasRightHand;
        public bool m_HasTDoF;

        public Human(UIReader reader)
        {
            var version = reader.Version;
            m_RootX = reader.ReadXForm();
            m_Skeleton = new Skeleton(reader);
            m_SkeletonPose = new SkeletonPose(reader);
            m_LeftHand = new Hand(reader);
            m_RightHand = new Hand(reader);

            if (version.LessThan(2018, 2)) //2018.2 down
            {
                int numHandles = reader.ReadInt32();
                m_Handles = new List<Handle>();
                for (int i = 0; i < numHandles; i++)
                {
                    m_Handles.Add(new Handle(reader));
                }

                int numColliders = reader.ReadInt32();
                m_ColliderArray = new List<Collider>(numColliders);
                for (int i = 0; i < numColliders; i++)
                {
                    m_ColliderArray.Add(new Collider(reader));
                }
            }

            m_HumanBoneIndex = reader.ReadInt32Array();

            m_HumanBoneMass = reader.ReadArray(r => r.ReadSingle());

            if (version.LessThan(2018, 2)) //2018.2 down
            {
                m_ColliderIndex = reader.ReadArray(r => r.ReadInt32());
            }

            m_Scale = reader.ReadSingle();
            m_ArmTwist = reader.ReadSingle();
            m_ForeArmTwist = reader.ReadSingle();
            m_UpperLegTwist = reader.ReadSingle();
            m_LegTwist = reader.ReadSingle();
            m_ArmStretch = reader.ReadSingle();
            m_LegStretch = reader.ReadSingle();
            m_FeetSpacing = reader.ReadSingle();
            m_HasLeftHand = reader.ReadBoolean();
            m_HasRightHand = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(5, 2)) //5.2 and up
            {
                m_HasTDoF = reader.ReadBoolean();
            }
            reader.AlignStream();
        }
    }

    public class AvatarConstant
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

        public AvatarConstant(UIReader reader)
        {
            var version = reader.Version;
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

    public sealed class Avatar : NamedObject
    {
        public uint m_AvatarSize;
        public AvatarConstant m_Avatar;
        public Dictionary<uint, string> m_TOS;

        public Avatar(UIReader reader) : base(reader)
        {
            m_AvatarSize = reader.ReadUInt32();
            m_Avatar = new AvatarConstant(reader);

            int numTOS = reader.ReadInt32();
            m_TOS = new Dictionary<uint, string>();
            for (int i = 0; i < numTOS; i++)
            {
                m_TOS.Add(reader.ReadUInt32(), reader.ReadAlignedString());
            }

            //HumanDescription m_HumanDescription 2019 and up
        }

        public string FindBonePath(uint hash)
        {
            m_TOS.TryGetValue(hash, out string path);
            return path;
        }
    }
}
