using System;
using System.Numerics;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class ClipMuscleConstantConverter : BundleConverter<ClipMuscleConstant>
    {

        public static void ReadBase(ClipMuscleConstant res, IBundleBinaryReader reader, 
            IBundleSerializer serializer, Action cb)
        {
            var version = reader.Get<Version>();
            if (version.LessThan(4, 3)) //4.3 down
            {
                var m_AdditionalCurveIndexArray = reader.ReadArray(r => r.ReadInt32());
            }
            res.ValueArrayDelta = reader.ReadArray<ValueDelta>(serializer);
            if (version.GreaterThanOrEquals(5, 3))//5.3 and up
            {
                res.ValueArrayReferencePose = reader.ReadArray(r => r.ReadSingle());
            }

            res.Mirror = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                res.LoopTime = reader.ReadBoolean();
            }
            res.LoopBlend = reader.ReadBoolean();
            res.LoopBlendOrientation = reader.ReadBoolean();
            res.LoopBlendPositionY = reader.ReadBoolean();
            res.LoopBlendPositionXZ = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(5, 5))//5.5 and up
            {
                res.StartAtOrigin = reader.ReadBoolean();
            }
            res.KeepOriginalOrientation = reader.ReadBoolean();
            res.KeepOriginalPositionY = reader.ReadBoolean();
            res.KeepOriginalPositionXZ = reader.ReadBoolean();
            res.HeightFromFeet = reader.ReadBoolean();
            reader.AlignStream();
            cb.Invoke();
        }

        public override ClipMuscleConstant? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new ClipMuscleConstant();
            res.DeltaPose = serializer.Deserialize<HumanPose>(reader);
            res.StartX = reader.ReadXForm();
            if (version.GreaterThanOrEquals(5, 5))//5.5 and up
            {
                res.StopX = reader.ReadXForm();
            }
            res.LeftFootStartX = reader.ReadXForm();
            res.RightFootStartX = reader.ReadXForm();
            if (version.LessThan(5))//5.0 down
            {
                res.MotionStartX = reader.ReadXForm();
                res.MotionStopX = reader.ReadXForm();
            }
            res.AverageSpeed = version.GreaterThanOrEquals(5, 4) ? reader.ReadVector3Or4() : 
                reader.ReadVector4().AsVector3();//5.4 and up
            res.Clip = serializer.Deserialize<Clip>(reader);
            res.StartTime = reader.ReadSingle();
            res.StopTime = reader.ReadSingle();
            res.OrientationOffsetY = reader.ReadSingle();
            res.Level = reader.ReadSingle();
            res.CycleOffset = reader.ReadSingle();
            res.AverageAngularSpeed = reader.ReadSingle();

            res.IndexArray = reader.ReadArray(r => r.ReadInt32());
            ReadBase(res, reader, serializer, () => { });
            return res;
        }


        private int ToSerializedVersion(int[] version)
        {
            if (version[0] > 5 || version[0] == 5 && version[1] >= 6)
            {
                return 3;
            }
            else if (version[0] > 4 || version[0] == 4 && version[1] >= 3)
            {
                return 2;
            }
            return 1;
        }
    }
}
