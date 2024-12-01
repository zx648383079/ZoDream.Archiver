using System.Collections.Generic;
using System.IO;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public partial class MiHoYoElementScanner
    {

        private void GICreateInstance(IBundleBinaryReader reader,
            HumanGoal instance)
        {
            instance.m_X = UnityReaderExtension.Parse(reader.ReadXForm4());
            instance.m_WeightT = reader.ReadSingle();
            instance.m_WeightR = reader.ReadSingle();

            instance.m_HintT = UnityReaderExtension.Parse(reader.ReadVector4());
            instance.m_HintWeightT = reader.ReadSingle();

            var m_HintR = UnityReaderExtension.Parse(reader.ReadVector4());
            var m_HintWeightR = reader.ReadSingle();
        }
        private void GICreateInstance(IBundleBinaryReader reader,
            HandPose instance)
        {
            instance.m_GrabX = UnityReaderExtension.Parse(reader.ReadXForm4());
            instance.m_DoFArray = reader.ReadArray(20, (r, _) => r.ReadSingle());
            instance.m_Override = reader.ReadSingle();
            instance.m_CloseOpen = reader.ReadSingle();
            instance.m_InOut = reader.ReadSingle();
            instance.m_Grab = reader.ReadSingle();
        }
        private void GICreateInstance(IBundleBinaryReader reader, HumanPose instance)
        {
            var version = reader.Get<UnityVersion>();
            instance.m_RootX = UnityReaderExtension.Parse(reader.ReadXForm4());
            instance.m_LookAtPosition = UnityReaderExtension.Parse(reader.ReadVector4());
            instance.m_LookAtWeight = reader.ReadVector4();

            instance.m_GoalArray = [];
            for (int i = 0; i < 4; i++)
            {
                var h = new HumanGoal();
                GICreateInstance(reader, h);
                instance.m_GoalArray.Add(h);
            }

            instance.m_LeftHandPose = new();
            GICreateInstance(reader, instance.m_LeftHandPose);
            instance.m_RightHandPose = new();
            GICreateInstance(reader, instance.m_RightHandPose);

            instance.m_DoFArray = reader.ReadArray(0x37, (r, _) => r.ReadSingle());

            instance.m_TDoFArray = reader.ReadArray(0x15, (_, _) => UnityReaderExtension.Parse(reader.ReadVector4()));

            reader.Position += 4;
        }
        private void GICreateInstance(IBundleBinaryReader reader,
            Clip instance)
        {
            var clipOffset = reader.Position + reader.ReadInt64();
            if (clipOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            var pos = reader.Position;
            reader.Position = clipOffset;

            instance.m_StreamedClip = new();
            GICreateInstance(reader, instance.m_StreamedClip);
            instance.m_DenseClip = new();
            GICreateInstance(reader, instance.m_DenseClip);

            instance.m_ConstantClip = new();
            GICreateInstance(reader, instance.m_ConstantClip);

            instance.m_ACLClip = new GIACLClip();
            instance.m_ACLClip.Read(reader);

            reader.Position = pos;
        }
        private void GICreateInstance(IBundleBinaryReader reader,
            StreamedClip instance)
        {
            var streamedClipCount = (int)reader.ReadUInt64();
            var streamedClipOffset = reader.Position + reader.ReadInt64();
            var streamedClipCurveCount = (uint)reader.ReadUInt64();
            if (streamedClipOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            var pos = reader.Position;
            reader.Position = streamedClipOffset;

            instance.data = reader.ReadArray(streamedClipCount, (r, _) => r.ReadUInt32());
            instance.curveCount = streamedClipCurveCount;

            reader.Position = pos;
        }
        private void GICreateInstance(IBundleBinaryReader reader,
            DenseClip instance)
        {
            instance.m_FrameCount = reader.ReadInt32();
            instance.m_CurveCount = reader.ReadUInt32();
            instance.m_SampleRate = reader.ReadSingle();
            instance.m_BeginTime = reader.ReadSingle();

            var denseClipCount = (int)reader.ReadUInt64();
            var denseClipOffset = reader.Position + reader.ReadInt64();
            if (denseClipOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            var pos = reader.Position;
            reader.Position = denseClipOffset;

            instance.m_SampleArray = reader.ReadArray(denseClipCount, (r, _) => r.ReadSingle());

            reader.Position = pos;
        }
        private void GICreateInstance(IBundleBinaryReader reader,
            ConstantClip instance)
        {
            var constantClipCount = (int)reader.ReadUInt64();
            var constantClipOffset = reader.Position + reader.ReadInt64();
            if (constantClipOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            var pos = reader.Position;
            reader.Position = constantClipOffset;

            instance.data = reader.ReadArray(constantClipCount, (r, _) => r.ReadSingle());

            reader.Position = pos;
        }
        private void GICreateInstance(IBundleBinaryReader reader, ClipMuscleConstant instance)
        {
            var version = reader.Get<UnityVersion>();
            instance.m_DeltaPose = new();
            GICreateInstance(reader, instance.m_DeltaPose);
            instance.m_StartX = UnityReaderExtension.Parse(reader.ReadXForm4());
            instance.m_StopX = UnityReaderExtension.Parse(reader.ReadXForm4());
            instance.m_LeftFootStartX = UnityReaderExtension.Parse(reader.ReadXForm4());
            instance.m_RightFootStartX = UnityReaderExtension.Parse(reader.ReadXForm4());

            instance.m_AverageSpeed = UnityReaderExtension.Parse(reader.ReadVector4());

            instance.m_Clip = new();
            GICreateInstance(reader, instance.m_Clip);

            instance.m_StartTime = reader.ReadSingle();
            instance.m_StopTime = reader.ReadSingle();
            instance.m_OrientationOffsetY = reader.ReadSingle();
            instance.m_Level = reader.ReadSingle();
            instance.m_CycleOffset = reader.ReadSingle();
            instance.m_AverageAngularSpeed = reader.ReadSingle();

            instance.m_IndexArray = reader.ReadArray(0xC8, (r, _) => (int)r.ReadInt16());

            var valueArrayDeltaCount = (int)reader.ReadUInt64();
            var valueArrayDeltaOffset = reader.Position + reader.ReadInt64();

            if (valueArrayDeltaOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            var valueArrayReferencePoseCount = (int)reader.ReadUInt64();
            var valueArrayReferencePoseOffset = reader.Position + reader.ReadInt64();

            if (valueArrayReferencePoseOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            instance.m_Mirror = reader.ReadBoolean();
            instance.m_LoopTime = reader.ReadBoolean();
            instance.m_LoopBlend = reader.ReadBoolean();
            instance.m_LoopBlendOrientation = reader.ReadBoolean();
            instance.m_LoopBlendPositionY = reader.ReadBoolean();
            instance.m_LoopBlendPositionXZ = reader.ReadBoolean();
            instance.m_StartAtOrigin = reader.ReadBoolean();
            instance.m_KeepOriginalOrientation = reader.ReadBoolean();
            instance.m_KeepOriginalPositionY = reader.ReadBoolean();
            instance.m_KeepOriginalPositionXZ = reader.ReadBoolean();
            instance.m_HeightFromFeet = reader.ReadBoolean();
            reader.AlignStream();

            if (valueArrayDeltaCount > 0)
            {
                reader.Position = valueArrayDeltaOffset;
                instance.m_ValueArrayDelta = new List<ValueDelta>();
                for (int i = 0; i < valueArrayDeltaCount; i++)
                {
                    instance.m_ValueArrayDelta.Add(new ValueDelta(reader));
                }
            }

            if (valueArrayReferencePoseCount > 0)
            {
                reader.Position = valueArrayReferencePoseOffset;
                instance.m_ValueArrayReferencePose = reader.ReadArray(valueArrayReferencePoseCount, (r, _) => r.ReadSingle());
            }

        }
    }
}
