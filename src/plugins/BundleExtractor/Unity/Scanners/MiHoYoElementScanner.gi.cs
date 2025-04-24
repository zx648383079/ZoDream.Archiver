using System;
using System.IO;
using System.Numerics;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class MiHoYoElementScanner
    {
        private HumanGoal ReadGIHumanGoal(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new HumanGoal
            {
                X = reader.ReadXForm4(),
                WeightT = reader.ReadSingle(),
                WeightR = reader.ReadSingle(),

                HintT = reader.ReadVector4().AsVector3(),
                HintWeightT = reader.ReadSingle()
            };

            var m_HintR = reader.ReadVector4().AsVector3();
            var m_HintWeightR = reader.ReadSingle();
            return res;
        }

        private HandPose ReadGIHandPose(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new HandPose
            {
                GrabX = reader.ReadXForm4(),
                DoFArray = reader.ReadArray(20, r => r.ReadSingle()),
                Override = reader.ReadSingle(),
                CloseOpen = reader.ReadSingle(),
                InOut = reader.ReadSingle(),
                Grab = reader.ReadSingle()
            };
            return res;
        }

        private HumanPose ReadGIHumanPose(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new HumanPose();
            var version = reader.Get<Version>();
            res.RootX = reader.ReadXForm4();
            res.LookAtPosition = reader.ReadVector4().AsVector3();
            res.LookAtWeight = reader.ReadVector4();

            res.GoalArray = reader.ReadArray(4, _ => serializer.Deserialize<HumanGoal>(reader));

            res.LeftHandPose = serializer.Deserialize<HandPose>(reader);
            res.RightHandPose = serializer.Deserialize<HandPose>(reader);

            res.DoFArray = reader.ReadArray(0x37, r => r.ReadSingle());

            res.TDoFArray = reader.ReadArray(0x15, _ => reader.ReadVector4().AsVector3());

            reader.Position += 4;
            return res;
        }

        private Clip ReadGIClip(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var clipOffset = reader.Position + reader.ReadInt64();
            if (clipOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }
            var res = new Clip();
            var pos = reader.Position;
            reader.Position = clipOffset;

            res.StreamedClip = serializer.Deserialize<StreamedClip>(reader);
            res.DenseClip = serializer.Deserialize<DenseClip>(reader); ;

            res.ConstantClip = serializer.Deserialize<ConstantClip>(reader);

            res.ACLClip = serializer.Deserialize<GIACLClip>(reader);

            reader.Position = pos;
            return res;
        }
        private StreamedClip ReadGIStreamedClip(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var streamedClipCount = (int)reader.ReadUInt64();
            var streamedClipOffset = reader.Position + reader.ReadInt64();
            var streamedClipCurveCount = (uint)reader.ReadUInt64();
            if (streamedClipOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }
            var res = new StreamedClip();
            var pos = reader.Position;
            reader.Position = streamedClipOffset;

            res.Data = reader.ReadArray(streamedClipCount, (r, _) => r.ReadUInt32());
            res.CurveCount = streamedClipCurveCount;

            reader.Position = pos;
            return res;
        }
    
        private DenseClip ReadGIDenseClip(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new DenseClip();
            res.FrameCount = reader.ReadInt32();
            res.CurveCount = reader.ReadUInt32();
            res.SampleRate = reader.ReadSingle();
            res.BeginTime = reader.ReadSingle();

            var denseClipCount = (int)reader.ReadUInt64();
            var denseClipOffset = reader.Position + reader.ReadInt64();
            if (denseClipOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            var pos = reader.Position;
            reader.Position = denseClipOffset;

            res.SampleArray = reader.ReadArray(denseClipCount, (r, _) => r.ReadSingle());

            reader.Position = pos;
            return res;
        }
    
        private ConstantClip ReadGIConstantClip(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var constantClipCount = (int)reader.ReadUInt64();
            var constantClipOffset = reader.Position + reader.ReadInt64();
            if (constantClipOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }
            var res = new ConstantClip();
            var pos = reader.Position;
            reader.Position = constantClipOffset;

            res.Data = reader.ReadArray(constantClipCount, (r, _) => r.ReadSingle());

            reader.Position = pos;
            return res;
        }
        private ClipMuscleConstant ReadGIClipMuscleConstant(IBundleBinaryReader reader, IBundleSerializer serializer)
        {
            var res = new ClipMuscleConstant();
            var version = reader.Get<Version>();
            res.DeltaPose = serializer.Deserialize<HumanPose>(reader);
            res.StartX = reader.ReadXForm4();
            res.StopX = reader.ReadXForm4();
            res.LeftFootStartX = reader.ReadXForm4();
            res.RightFootStartX = reader.ReadXForm4();

            res.AverageSpeed = reader.ReadVector4().AsVector3();

            res.Clip = serializer.Deserialize<Clip>(reader);

            res.StartTime = reader.ReadSingle();
            res.StopTime = reader.ReadSingle();
            res.OrientationOffsetY = reader.ReadSingle();
            res.Level = reader.ReadSingle();
            res.CycleOffset = reader.ReadSingle();
            res.AverageAngularSpeed = reader.ReadSingle();

            res.IndexArray = reader.ReadArray(0xC8, (r, _) => (int)r.ReadInt16());

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

            res.Mirror = reader.ReadBoolean();
            res.LoopTime = reader.ReadBoolean();
            res.LoopBlend = reader.ReadBoolean();
            res.LoopBlendOrientation = reader.ReadBoolean();
            res.LoopBlendPositionY = reader.ReadBoolean();
            res.LoopBlendPositionXZ = reader.ReadBoolean();
            res.StartAtOrigin = reader.ReadBoolean();
            res.KeepOriginalOrientation = reader.ReadBoolean();
            res.KeepOriginalPositionY = reader.ReadBoolean();
            res.KeepOriginalPositionXZ = reader.ReadBoolean();
            res.HeightFromFeet = reader.ReadBoolean();
            reader.AlignStream();

            if (valueArrayDeltaCount > 0)
            {
                reader.Position = valueArrayDeltaOffset;
                res.ValueArrayDelta = reader.ReadArray<ValueDelta>(valueArrayDeltaCount, serializer);
            }

            if (valueArrayReferencePoseCount > 0)
            {
                reader.Position = valueArrayReferencePoseOffset;
                res.ValueArrayReferencePose = reader.ReadArray(valueArrayReferencePoseCount, r => r.ReadSingle());
            }
            return res;
        }
    }

}
