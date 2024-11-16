using System.Collections.Generic;
using System.Numerics;

namespace ZoDream.BundleExtractor.Unity.UI
{
    public class HumanPoseMask
    {
        public uint word0;
        public uint word1;
        public uint word2;

        public HumanPoseMask(UIReader reader)
        {
            var version = reader.Version;

            word0 = reader.Reader.ReadUInt32();
            word1 = reader.Reader.ReadUInt32();
            if (version.GreaterThanOrEquals(5, 2)) //5.2 and up
            {
                word2 = reader.Reader.ReadUInt32();
            }
        }
    }

    public class SkeletonMaskElement
    {
        public uint m_PathHash;
        public float m_Weight;

        public SkeletonMaskElement(UIReader reader)
        {
            m_PathHash = reader.Reader.ReadUInt32();
            m_Weight = reader.Reader.ReadSingle();
        }
    }

    public class SkeletonMask
    {
        public List<SkeletonMaskElement> m_Data;

        public SkeletonMask(UIReader reader)
        {
            int numElements = reader.Reader.ReadInt32();
            m_Data = new List<SkeletonMaskElement>();
            for (int i = 0; i < numElements; i++)
            {
                m_Data.Add(new SkeletonMaskElement(reader));
            }
        }
    }

    public class LayerConstant
    {
        public uint m_StateMachineIndex;
        public uint m_StateMachineMotionSetIndex;
        public HumanPoseMask m_BodyMask;
        public SkeletonMask m_SkeletonMask;
        public uint m_Binding;
        public int m_LayerBlendingMode;
        public float m_DefaultWeight;
        public bool m_IKPass;
        public bool m_SyncedLayerAffectsTiming;

        public LayerConstant(UIReader reader)
        {
            var version = reader.Version;

            m_StateMachineIndex = reader.Reader.ReadUInt32();
            m_StateMachineMotionSetIndex = reader.Reader.ReadUInt32();
            m_BodyMask = new HumanPoseMask(reader);
            m_SkeletonMask = new SkeletonMask(reader);
            if (reader.IsLoveAndDeepSpace())
            {
                var m_GenericMask = new SkeletonMask(reader);
            }
            m_Binding = reader.Reader.ReadUInt32();
            m_LayerBlendingMode = reader.Reader.ReadInt32();
            if (version.GreaterThanOrEquals(4, 2)) //4.2 and up
            {
                m_DefaultWeight = reader.Reader.ReadSingle();
            }
            m_IKPass = reader.Reader.ReadBoolean();
            if (version.GreaterThanOrEquals(4, 2)) //4.2 and up
            {
                m_SyncedLayerAffectsTiming = reader.Reader.ReadBoolean();
            }
            reader.Reader.AlignStream();
        }

    }

    public class ConditionConstant
    {
        public uint m_ConditionMode;
        public uint m_EventID;
        public float m_EventThreshold;
        public float m_ExitTime;

        public ConditionConstant(UIReader reader)
        {
            m_ConditionMode = reader.Reader.ReadUInt32();
            m_EventID = reader.Reader.ReadUInt32();
            m_EventThreshold = reader.Reader.ReadSingle();
            m_ExitTime = reader.Reader.ReadSingle();
        }
    }

    public class TransitionConstant
    {
        public List<ConditionConstant> m_ConditionConstantArray;
        public uint m_DestinationState;
        public uint m_FullPathID;
        public uint m_ID;
        public uint m_UserID;
        public float m_TransitionDuration;
        public float m_TransitionOffset;
        public float m_ExitTime;
        public bool m_HasExitTime;
        public bool m_HasFixedDuration;
        public int m_InterruptionSource;
        public bool m_OrderedInterruption;
        public bool m_Atomic;
        public bool m_CanTransitionToSelf;

        public TransitionConstant(UIReader reader)
        {
            var version = reader.Version;

            int numConditions = reader.Reader.ReadInt32();
            m_ConditionConstantArray = new List<ConditionConstant>();
            for (int i = 0; i < numConditions; i++)
            {
                m_ConditionConstantArray.Add(new ConditionConstant(reader));
            }

            m_DestinationState = reader.Reader.ReadUInt32();
            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                m_FullPathID = reader.Reader.ReadUInt32();
            }

            m_ID = reader.Reader.ReadUInt32();
            m_UserID = reader.Reader.ReadUInt32();
            m_TransitionDuration = reader.Reader.ReadSingle();
            m_TransitionOffset = reader.Reader.ReadSingle();
            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                m_ExitTime = reader.Reader.ReadSingle();
                m_HasExitTime = reader.Reader.ReadBoolean();
                m_HasFixedDuration = reader.Reader.ReadBoolean();
                reader.Reader.AlignStream();
                m_InterruptionSource = reader.Reader.ReadInt32();
                m_OrderedInterruption = reader.Reader.ReadBoolean();
            }
            else
            {
                m_Atomic = reader.Reader.ReadBoolean();
            }

            if (version.GreaterThanOrEquals(4, 5)) //4.5 and up
            {
                m_CanTransitionToSelf = reader.Reader.ReadBoolean();
            }

            reader.Reader.AlignStream();
        }
    }

    public class LeafInfoConstant
    {
        public uint[] m_IDArray;
        public uint m_IndexOffset;

        public LeafInfoConstant(UIReader reader)
        {
            m_IDArray = reader.Reader.ReadArray(r => r.ReadUInt32());
            m_IndexOffset = reader.Reader.ReadUInt32();
        }
    }

    public class MotionNeighborList
    {
        public uint[] m_NeighborArray;

        public MotionNeighborList(UIReader reader)
        {
            m_NeighborArray = reader.Reader.ReadArray(r => r.ReadUInt32());
        }
    }

    public class Blend2dDataConstant
    {
        public Vector2[] m_ChildPositionArray;
        public float[] m_ChildMagnitudeArray;
        public Vector2[] m_ChildPairVectorArray;
        public float[] m_ChildPairAvgMagInvArray;
        public List<MotionNeighborList> m_ChildNeighborListArray;

        public Blend2dDataConstant(UIReader reader)
        {
            m_ChildPositionArray = reader.Reader.ReadArray(_ => reader.ReadVector2());
            m_ChildMagnitudeArray = reader.Reader.ReadArray(r => r.ReadSingle());
            m_ChildPairVectorArray = reader.Reader.ReadArray(_ => reader.ReadVector2());
            m_ChildPairAvgMagInvArray = reader.Reader.ReadArray(r => r.ReadSingle());

            int numNeighbours = reader.Reader.ReadInt32();
            m_ChildNeighborListArray = new List<MotionNeighborList>();
            for (int i = 0; i < numNeighbours; i++)
            {
                m_ChildNeighborListArray.Add(new MotionNeighborList(reader));
            }
        }
    }

    public class Blend1dDataConstant // wrong labeled
    {
        public float[] m_ChildThresholdArray;

        public Blend1dDataConstant(UIReader reader)
        {
            m_ChildThresholdArray = reader.Reader.ReadArray(r => r.ReadSingle());
        }
    }

    public class BlendDirectDataConstant
    {
        public uint[] m_ChildBlendEventIDArray;
        public bool m_NormalizedBlendValues;

        public BlendDirectDataConstant(UIReader reader)
        {
            m_ChildBlendEventIDArray = reader.Reader.ReadArray(r => r.ReadUInt32());
            m_NormalizedBlendValues = reader.Reader.ReadBoolean();
            reader.Reader.AlignStream();
        }
    }

    public class BlendTreeNodeConstant
    {
        public uint m_BlendType;
        public uint m_BlendEventID;
        public uint m_BlendEventYID;
        public uint[] m_ChildIndices;
        public float[] m_ChildThresholdArray;
        public Blend1dDataConstant m_Blend1dData;
        public Blend2dDataConstant m_Blend2dData;
        public BlendDirectDataConstant m_BlendDirectData;
        public uint m_ClipID;
        public uint m_ClipIndex;
        public float m_Duration;
        public float m_CycleOffset;
        public bool m_Mirror;

        public BlendTreeNodeConstant(UIReader reader)
        {
            var version = reader.Version;

            if (version.GreaterThanOrEquals(4, 1)) //4.1 and up
            {
                m_BlendType = reader.Reader.ReadUInt32();
            }
            m_BlendEventID = reader.Reader.ReadUInt32();
            if (version.GreaterThanOrEquals(4, 1)) //4.1 and up
            {
                m_BlendEventYID = reader.Reader.ReadUInt32();
            }
            m_ChildIndices = reader.Reader.ReadArray(r => r.ReadUInt32());
            if (version.LessThan(4, 1)) //4.1 down
            {
                m_ChildThresholdArray = reader.Reader.ReadArray(r => r.ReadSingle());
            }

            if (version.GreaterThanOrEquals(4, 1)) //4.1 and up
            {
                m_Blend1dData = new Blend1dDataConstant(reader);
                m_Blend2dData = new Blend2dDataConstant(reader);
            }

            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                m_BlendDirectData = new BlendDirectDataConstant(reader);
            }

            m_ClipID = reader.Reader.ReadUInt32();
            if (version.Major == 4 && version.Minor >= 5) //4.5 - 5.0
            {
                m_ClipIndex = reader.Reader.ReadUInt32();
            }

            m_Duration = reader.Reader.ReadSingle();

            if (version.GreaterThanOrEquals(4, 1, 3)) //4.1.3 and up
            {
                m_CycleOffset = reader.Reader.ReadSingle();
                if (reader.IsArknightsEndfield())
                {
                    var m_StateNameHash = reader.Reader.ReadUInt32();
                }
                m_Mirror = reader.Reader.ReadBoolean();
                reader.Reader.AlignStream();
            }
        }
    }

    public class BlendTreeConstant
    {
        public List<BlendTreeNodeConstant> m_NodeArray;
        public ValueArrayConstant m_BlendEventArrayConstant;

        public BlendTreeConstant(UIReader reader)
        {
            var version = reader.Version;

            int numNodes = reader.Reader.ReadInt32();
            m_NodeArray = new List<BlendTreeNodeConstant>();
            for (int i = 0; i < numNodes; i++)
            {
                m_NodeArray.Add(new BlendTreeNodeConstant(reader));
            }

            if (version.LessThan(4, 5)) //4.5 down
            {
                m_BlendEventArrayConstant = new ValueArrayConstant(reader);
            }
        }
    }


    public class StateConstant
    {
        public List<TransitionConstant> m_TransitionConstantArray;
        public int[] m_BlendTreeConstantIndexArray;
        public List<LeafInfoConstant> m_LeafInfoArray;
        public List<BlendTreeConstant> m_BlendTreeConstantArray;
        public uint m_NameID;
        public uint m_PathID;
        public uint m_FullPathID;
        public uint m_TagID;
        public uint m_SpeedParamID;
        public uint m_MirrorParamID;
        public uint m_CycleOffsetParamID;
        public float m_Speed;
        public float m_CycleOffset;
        public bool m_IKOnFeet;
        public bool m_WriteDefaultValues;
        public bool m_Loop;
        public bool m_Mirror;

        public StateConstant(UIReader reader)
        {
            var version = reader.Version;

            int numTransistions = reader.Reader.ReadInt32();
            m_TransitionConstantArray = new List<TransitionConstant>();
            for (int i = 0; i < numTransistions; i++)
            {
                m_TransitionConstantArray.Add(new TransitionConstant(reader));
            }

            m_BlendTreeConstantIndexArray = reader.Reader.ReadArray(r => r.ReadInt32());

            if (version.LessThan(5, 2)) //5.2 down
            {
                int numInfos = reader.Reader.ReadInt32();
                m_LeafInfoArray = new List<LeafInfoConstant>();
                for (int i = 0; i < numInfos; i++)
                {
                    m_LeafInfoArray.Add(new LeafInfoConstant(reader));
                }
            }

            int numBlends = reader.Reader.ReadInt32();
            m_BlendTreeConstantArray = new List<BlendTreeConstant>();
            for (int i = 0; i < numBlends; i++)
            {
                m_BlendTreeConstantArray.Add(new BlendTreeConstant(reader));
            }

            m_NameID = reader.Reader.ReadUInt32();
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                m_PathID = reader.Reader.ReadUInt32();
            }
            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                m_FullPathID = reader.Reader.ReadUInt32();
            }

            m_TagID = reader.Reader.ReadUInt32();
            if (version.GreaterThanOrEquals(5, 1)) //5.1 and up
            {
                m_SpeedParamID = reader.Reader.ReadUInt32();
                m_MirrorParamID = reader.Reader.ReadUInt32();
                m_CycleOffsetParamID = reader.Reader.ReadUInt32();
            }

            if (version.GreaterThanOrEquals(2017, 2)) //2017.2 and up
            {
                var m_TimeParamID = reader.Reader.ReadUInt32();
            }

            m_Speed = reader.Reader.ReadSingle();
            if (version.GreaterThanOrEquals(4, 1)) //4.1 and up
            {
                m_CycleOffset = reader.Reader.ReadSingle();
            }
            m_IKOnFeet = reader.Reader.ReadBoolean();
            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                m_WriteDefaultValues = reader.Reader.ReadBoolean();
            }

            m_Loop = reader.Reader.ReadBoolean();
            if (version.GreaterThanOrEquals(4, 1)) //4.1 and up
            {
                m_Mirror = reader.Reader.ReadBoolean();
            }

            if (reader.IsArknightsEndfield())
            {
                var m_SyncGroupID = reader.Reader.ReadUInt32();
                var m_SyncGroupRole = reader.Reader.ReadUInt32();
            }

            reader.Reader.AlignStream();
        }
    }

    public class SelectorTransitionConstant
    {
        public uint m_Destination;
        public List<ConditionConstant> m_ConditionConstantArray;

        public SelectorTransitionConstant(UIReader reader)
        {
            m_Destination = reader.Reader.ReadUInt32();

            int numConditions = reader.Reader.ReadInt32();
            m_ConditionConstantArray = new List<ConditionConstant>();
            for (int i = 0; i < numConditions; i++)
            {
                m_ConditionConstantArray.Add(new ConditionConstant(reader));
            }
        }
    }

    public class SelectorStateConstant
    {
        public List<SelectorTransitionConstant> m_TransitionConstantArray;
        public uint m_FullPathID;
        public bool m_isEntry;

        public SelectorStateConstant(UIReader reader)
        {
            int numTransitions = reader.Reader.ReadInt32();
            m_TransitionConstantArray = new List<SelectorTransitionConstant>();
            for (int i = 0; i < numTransitions; i++)
            {
                m_TransitionConstantArray.Add(new SelectorTransitionConstant(reader));
            }

            m_FullPathID = reader.Reader.ReadUInt32();
            m_isEntry = reader.Reader.ReadBoolean();
            reader.Reader.AlignStream();
        }
    }

    public class StateMachineConstant
    {
        public List<StateConstant> m_StateConstantArray;
        public List<TransitionConstant> m_AnyStateTransitionConstantArray;
        public List<SelectorStateConstant> m_SelectorStateConstantArray;
        public uint m_DefaultState;
        public uint m_MotionSetCount;

        public StateMachineConstant(UIReader reader)
        {
            var version = reader.Version;

            int numStates = reader.Reader.ReadInt32();
            m_StateConstantArray = new List<StateConstant>();
            for (int i = 0; i < numStates; i++)
            {
                m_StateConstantArray.Add(new StateConstant(reader));
            }

            int numAnyStates = reader.Reader.ReadInt32();
            m_AnyStateTransitionConstantArray = new List<TransitionConstant>();
            for (int i = 0; i < numAnyStates; i++)
            {
                m_AnyStateTransitionConstantArray.Add(new TransitionConstant(reader));
            }

            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                int numSelectors = reader.Reader.ReadInt32();
                m_SelectorStateConstantArray = new List<SelectorStateConstant>();
                for (int i = 0; i < numSelectors; i++)
                {
                    m_SelectorStateConstantArray.Add(new SelectorStateConstant(reader));
                }
            }

            m_DefaultState = reader.Reader.ReadUInt32();
            m_MotionSetCount = reader.Reader.ReadUInt32();
        }
    }

    public class ValueArray
    {
        public bool[] m_BoolValues;
        public int[] m_IntValues;
        public float[] m_FloatValues;
        public Vector4[] m_VectorValues;
        public Vector3[] m_PositionValues;
        public Vector4[] m_QuaternionValues;
        public Vector3[] m_ScaleValues;

        public ValueArray(UIReader reader)
        {
            var version = reader.Version;

            if (version.LessThan(5, 5)) //5.5 down
            {
                m_BoolValues = reader.Reader.ReadArray(r => r.ReadBoolean());
                reader.Reader.AlignStream();
                m_IntValues = reader.Reader.ReadInt32Array();
                m_FloatValues = reader.Reader.ReadArray(r => r.ReadSingle());
            }

            if (version.GreaterThan(4, 3)) //4.3 down
            {
                m_VectorValues = reader.Reader.ReadArray(_ => reader.ReadVector4());
            }
            else
            {
                m_PositionValues = reader.ReadVector3Array();

                m_QuaternionValues = reader.Reader.ReadArray(_ => reader.ReadVector4());

                m_ScaleValues = reader.ReadVector3Array();

                if (version.GreaterThanOrEquals(5, 5)) //5.5 and up
                {
                    m_FloatValues = reader.Reader.ReadArray(r => r.ReadSingle());
                    m_IntValues = reader.Reader.ReadInt32Array();
                    m_BoolValues = reader.Reader.ReadArray(r => r.ReadBoolean());
                    reader.Reader.AlignStream();
                }
            }
        }
    }

    public class ControllerConstant
    {
        public List<LayerConstant> m_LayerArray;
        public List<StateMachineConstant> m_StateMachineArray;
        public ValueArrayConstant m_Values;
        public ValueArray m_DefaultValues;

        public ControllerConstant(UIReader reader)
        {
            int numLayers = reader.Reader.ReadInt32();
            m_LayerArray = new List<LayerConstant>();
            for (int i = 0; i < numLayers; i++)
            {
                m_LayerArray.Add(new LayerConstant(reader));
            }

            int numStates = reader.Reader.ReadInt32();
            m_StateMachineArray = new List<StateMachineConstant>();
            for (int i = 0; i < numStates; i++)
            {
                m_StateMachineArray.Add(new StateMachineConstant(reader));
            }

            m_Values = new ValueArrayConstant(reader);
            m_DefaultValues = new ValueArray(reader);
        }
    }

    public sealed class AnimatorController : RuntimeAnimatorController
    {
        public Dictionary<uint, string> m_TOS;
        public List<PPtr<AnimationClip>> m_AnimationClips;

        public AnimatorController(UIReader reader) : base(reader)
        {
            var m_ControllerSize = reader.Reader.ReadUInt32();
            var m_Controller = new ControllerConstant(reader);

            int tosSize = reader.Reader.ReadInt32();
            m_TOS = new Dictionary<uint, string>();
            for (int i = 0; i < tosSize; i++)
            {
                m_TOS.Add(reader.Reader.ReadUInt32(), reader.ReadAlignedString());
            }

            int numClips = reader.Reader.ReadInt32();
            m_AnimationClips = new List<PPtr<AnimationClip>>();
            for (int i = 0; i < numClips; i++)
            {
                m_AnimationClips.Add(new PPtr<AnimationClip>(reader));
            }
        }
    }
}
